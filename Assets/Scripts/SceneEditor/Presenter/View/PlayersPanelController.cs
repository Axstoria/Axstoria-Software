using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Loxodon.Framework.Contexts;
using MapEditor.Domain;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using SceneEditor.Presenter.ViewModels;
using UnityEngine;
using UnityEngine.UIElements;

namespace SceneEditor.Presenter.View
{
    public class PlayersPanelController : MonoBehaviour
    {
        private TextField _newNameField;
        private Button _addButton;
        private VisualElement _playerList;

        private ColorPickerPopupController _colorPicker;
        private MapEditorViewModel _vm;
        private NotifyCollectionChangedEventHandler _onPlayersChanged;
        private NotifyCollectionChangedEventHandler _onObjectsChanged;
        private readonly Dictionary<Player, EventHandler> _metadataHandlers = new();
        private readonly Dictionary<ObjectViewModel, EventHandler> _pawnHandlers = new();

        public void Init(VisualElement root, ColorPickerPopupController colorPicker)
        {
            _colorPicker = colorPicker;

            _newNameField = root.Q<TextField>("new-player-name-field");
            _addButton    = root.Q<Button>("add-player-button");
            _playerList   = root.Q<VisualElement>("player-list");

            _addButton.clicked += OnAddPlayer;

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogWarning("[PlayersPanelController] MapEditorViewModel not registered");
                return;
            }

            SubscribeToPawnChanges();
            RebuildList();
            _onPlayersChanged = (_, __) => RebuildList();
            _onObjectsChanged = (_, __) => { SubscribeToPawnChanges(); RebuildList(); };
            _vm.Map.Players.CollectionChanged += _onPlayersChanged;
            _vm.Map.Objects.CollectionChanged += _onObjectsChanged;
        }

        private void OnDestroy()
        {
            if (_vm == null) return;
            if (_onPlayersChanged != null) _vm.Map.Players.CollectionChanged -= _onPlayersChanged;
            if (_onObjectsChanged != null) _vm.Map.Objects.CollectionChanged -= _onObjectsChanged;
            UnsubscribeAllRows();
            UnsubscribeAllPawnHandlers();
        }

        private void SubscribeToPawnChanges()
        {
            UnsubscribeAllPawnHandlers();
            foreach (ObjectViewModel obj in _vm.Map.Objects)
            {
                EventHandler onPawnChanged = (_, __) => RebuildList();
                obj.IsPawn.ValueChanged += onPawnChanged;
                _pawnHandlers[obj] = onPawnChanged;
            }
        }

        private void UnsubscribeAllPawnHandlers()
        {
            foreach (var kvp in _pawnHandlers)
                kvp.Key.IsPawn.ValueChanged -= kvp.Value;
            _pawnHandlers.Clear();
        }

        private void OnAddPlayer()
        {
            string name = _newNameField.value;
            if (string.IsNullOrWhiteSpace(name)) return;

            _vm.CreatePlayer.Execute(name);
            _newNameField.value = "";
        }

        private void RebuildList()
        {
            UnsubscribeAllRows();
            _playerList.Clear();

            foreach (PlayerViewModel player in _vm.Map.Players)
                AddRow(player);
        }

        private void UnsubscribeAllRows()
        {
            foreach (var kvp in _metadataHandlers)
                kvp.Key.OnMetadataChanged -= kvp.Value;
            _metadataHandlers.Clear();
        }

        private void AddRow(PlayerViewModel player)
        {
            var card = new VisualElement();
            card.AddToClassList("player-card");
            
            var headerRow = new VisualElement();
            headerRow.AddToClassList("player-header-row");

            var swatch = new VisualElement();
            swatch.AddToClassList("tag-manager-swatch");
            ApplySwatchColor(swatch, player.HexColor.Value);
            swatch.RegisterCallback<ClickEvent>(_ =>
                _colorPicker.Open(swatch, player.HexColor.Value, hex =>
                {
                    ApplySwatchColor(swatch, hex);
                    player.HexColor.Value = hex;
                }));

            var nameField = new TextField { value = player.Name.Value };
            nameField.AddToClassList("player-name-field");
            nameField.RegisterValueChangedCallback(e => player.Name.Value = e.newValue);

            var deleteButton = new Button { text = "×" };
            deleteButton.AddToClassList("tag-manager-delete-button");
            deleteButton.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                _vm.DeletePlayer.Execute(player.Model);
            });

            headerRow.Add(swatch);
            headerRow.Add(nameField);
            headerRow.Add(deleteButton);

            // ── Pawn assignment ──────────────────────────────────────────────
            var pawnRow = new VisualElement();
            pawnRow.AddToClassList("player-pawn-row");

            var pawnLabel = new Label("Pawn");
            pawnLabel.AddToClassList("field-label");

            var pawns = _vm.Map.Objects.Where(o => o.IsPawn.Value).ToList();
            var pawnDropdown = new DropdownField();
            pawnDropdown.AddToClassList("player-pawn-dropdown");
            var choices = new List<string> { "(none)" };
            choices.AddRange(pawns.Select(p => string.IsNullOrEmpty(p.DisplayName.Value) ? "(unnamed)" : p.DisplayName.Value));
            pawnDropdown.choices = choices;

            int currentIndex = pawns.FindIndex(p => p.Model.Id == player.Model.PawnId);
            pawnDropdown.index = currentIndex >= 0 ? currentIndex + 1 : 0;

            pawnDropdown.RegisterValueChangedCallback(e =>
            {
                int idx = pawnDropdown.index;
                player.PawnId.Value = (idx > 0 && idx - 1 < pawns.Count) ? pawns[idx - 1].Model.Id : "";
            });

            pawnRow.Add(pawnLabel);
            pawnRow.Add(pawnDropdown);

            // ── Tags ─────────────────────────────────────────────────────────
            var tagChipList = new VisualElement();
            tagChipList.AddToClassList("tag-manager-list");

            var tagAssignRow = new VisualElement();
            tagAssignRow.AddToClassList("player-pawn-row");

            var tagDropdown = new DropdownField();
            tagDropdown.AddToClassList("player-pawn-dropdown");
            var tags = new List<TagViewModel>(_vm.Map.Tags);
            tagDropdown.choices = tags.Select(t => t.Name.Value).ToList();
            tagDropdown.index = tags.Count > 0 ? 0 : -1;

            var tagAddButton = new Button { text = "Add" };
            tagAddButton.AddToClassList("settings-btn");
            tagAddButton.clicked += () =>
            {
                if (tagDropdown.index < 0) return;
                TagValue selected = tags[tagDropdown.index].Model;
                bool alreadyAssigned = player.Model.Metadata?.Any(e =>
                    e.EntryValue is TagValue tv && tv.Id == selected.Id) == true;
                if (!alreadyAssigned)
                    _vm.SetObjectMetadata.Execute(player.Model, "tag", new TagValue { Id = selected.Id });
            };

            tagAssignRow.Add(tagDropdown);
            tagAssignRow.Add(tagAddButton);

            EventHandler onMetadataChanged = (_, __) => RefreshTagChips(player, tagChipList);
            player.Model.OnMetadataChanged += onMetadataChanged;
            _metadataHandlers[player.Model] = onMetadataChanged;
            RefreshTagChips(player, tagChipList);

            card.Add(headerRow);
            card.Add(pawnRow);
            card.Add(tagChipList);
            card.Add(tagAssignRow);
            _playerList.Add(card);
        }

        private void RefreshTagChips(PlayerViewModel player, VisualElement tagChipList)
        {
            tagChipList.Clear();

            var entries = player.Model.Metadata?.Where(e => e.EntryValue is TagValue).ToList();
            if (entries == null) return;

            foreach (var entry in entries)
            {
                var tagValue = (TagValue)entry.EntryValue;
                var tagDef = _vm.Map.Tags.FirstOrDefault(t => t.Model.Id == tagValue.Id);

                var chip = new VisualElement();
                chip.AddToClassList("tag-chip");

                var swatch = new VisualElement();
                swatch.AddToClassList("tag-swatch");
                string hex = tagDef?.HexColor.Value ?? "#808080";
                if (ColorUtility.TryParseHtmlString(hex, out Color c))
                    swatch.style.backgroundColor = c;

                var label = new Label(tagDef?.Name.Value ?? "(unknown tag)");
                label.AddToClassList("tag-chip-name");

                var remove = new Button { text = "×" };
                remove.AddToClassList("tag-chip-remove");
                remove.RegisterCallback<ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    _vm.RemoveObjectMetadata.Execute(player.Model, entry);
                });

                chip.Add(swatch);
                chip.Add(label);
                chip.Add(remove);
                tagChipList.Add(chip);
            }
        }

        private static void ApplySwatchColor(VisualElement swatch, string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                swatch.style.backgroundColor = c;
        }
    }
}
