using System;
using System.Collections.Generic;
using System.Linq;
using Campaign.App.Port;
using Camera.Domain;
using MapEditor.Domain;
using SceneEditor.Domain;
using UnityEngine;
using DomainGrid = Grid.Domain.Grid;

namespace Campaign.Infrastructure
{
    public class JsonMapSerializer : IMapSerializer
    {
        public string Serialize(Map map)
        {
            return JsonUtility.ToJson(ToDTO(map), true);
        }

        public Map Deserialize(string json)
        {
            return FromDTO(JsonUtility.FromJson<MapDataDTO>(json));
        }

        // ── Map ↔ DTO ─────────────────────────────────────────────────────────

        private static MapDataDTO ToDTO(Map map)
        {
            var dto = new MapDataDTO
            {
                mapId   = map.Id,
                mapName = map.Name,
                savedAt = DateTime.UtcNow.ToString("o"),
                terrain = TerrainToDTO(map.TerrainLayout),
                objects = new List<SceneObjectDTO>(),
                metadata = map.Metadata?.Select(EntryToDTO).ToList() ?? new List<MetadataEntryDTO>(),
                players = map.Players?.Select(PlayerToDTO).ToList() ?? new List<PlayerDTO>(),
                light = LightToDTO(map.LightSettings),
                skyboxName = map.SkyboxName,
                cameraSettings = CameraSettingsToDTO(map.CameraSettings)
            };

            foreach (var obj in map.Objects)
                dto.objects.Add(ObjectToDTO(obj));

            return dto;
        }

        private static Map FromDTO(MapDataDTO dto)
        {
            var map = new Map
            {
                Id            = dto.mapId,
                Name          = dto.mapName,
                TerrainLayout = TerrainFromDTO(dto.terrain),
                Metadata      = dto.metadata?.Select(EntryFromDTO).ToList() ?? new List<MetadataEntry>(),
                LightSettings = LightFromDTO(dto.light),
                SkyboxName    = dto.skyboxName,
                CameraSettings = CameraSettingsFromDTO(dto.cameraSettings)
            };

            if (dto.objects != null)
                foreach (var objDTO in dto.objects)
                    map.Objects.Add(ObjectFromDTO(objDTO));

            if (dto.players != null)
                foreach (var playerDTO in dto.players)
                    map.Players.Add(PlayerFromDTO(playerDTO));

            return map;
        }

        private static PlayerDTO PlayerToDTO(Player player) => new PlayerDTO
        {
            id           = player.Id,
            name         = player.Name,
            isGameMaster = player.IsGameMaster,
            pawnId       = player.PawnId,
            hexColor     = player.HexColor,
            metadata = player.Metadata?.Select(EntryToDTO).ToList() ?? new List<MetadataEntryDTO>()
        };

        private static Player PlayerFromDTO(PlayerDTO dto) => new Player
        {
            Id           = dto.id,
            Name         = dto.name,
            IsGameMaster = dto.isGameMaster,
            PawnId       = dto.pawnId,
            HexColor     = string.IsNullOrEmpty(dto.hexColor) ? "#3399FF" : dto.hexColor,
            Metadata = dto.metadata?.Select(EntryFromDTO).ToList() ?? new List<MetadataEntry>()
        };

        private static TerrainDTO TerrainToDTO(TerrainLayout t)
        {
            if (t == null) return new TerrainDTO();
            return new TerrainDTO
            {
                width     = t.Width,
                depth     = t.Depth,
                thickness = t.Thickness,
                height    = t.Height,
                colorR    = t.Color != null && t.Color.Length > 0 ? t.Color[0] : 0.6f,
                colorG    = t.Color != null && t.Color.Length > 1 ? t.Color[1] : 0.4f,
                colorB    = t.Color != null && t.Color.Length > 2 ? t.Color[2] : 0.2f,
                colorA    = t.Color != null && t.Color.Length > 3 ? t.Color[3] : 1f,
                cellSize  = t.Grid?.CellSize ?? 1f,
                surfaceY  = t.Grid?.SurfaceY ?? 0f
            };
        }

        private static TerrainLayout TerrainFromDTO(TerrainDTO dto)
        {
            if (dto == null) return null;
            return new TerrainLayout
            {
                Width     = dto.width,
                Depth     = dto.depth,
                Thickness = dto.thickness,
                Height    = dto.height,
                Color     = new[] { dto.colorR, dto.colorG, dto.colorB, dto.colorA },
                Grid      = new DomainGrid { CellSize = dto.cellSize, SurfaceY = dto.surfaceY }
            };
        }

        private static LightDTO LightToDTO(LightSettings s)
        {
            if (s == null) return new LightDTO();
            return new LightDTO
            {
                intensity        = s.Intensity,
                colorR           = s.ColorR,
                colorG           = s.ColorG,
                colorB           = s.ColorB,
                pitch            = s.Pitch,
                yaw              = s.Yaw,
                shadowStrength   = s.ShadowStrength,
                ambientIntensity = s.AmbientIntensity,
                ambientColorR    = s.AmbientColorR,
                ambientColorG    = s.AmbientColorG,
                ambientColorB    = s.AmbientColorB
            };
        }

        private static LightSettings LightFromDTO(LightDTO dto)
        {
            if (dto == null) return new LightSettings();
            return new LightSettings
            {
                Intensity        = dto.intensity,
                ColorR           = dto.colorR,
                ColorG           = dto.colorG,
                ColorB           = dto.colorB,
                Pitch            = dto.pitch,
                Yaw              = dto.yaw,
                ShadowStrength   = dto.shadowStrength,
                AmbientIntensity = dto.ambientIntensity,
                AmbientColorR    = dto.ambientColorR,
                AmbientColorG    = dto.ambientColorG,
                AmbientColorB    = dto.ambientColorB
            };
        }

        private static CameraSettingsDTO CameraSettingsToDTO(CameraSettings s)
        {
            if (s == null) return new CameraSettingsDTO();
            return new CameraSettingsDTO
            {
                orbitSensitivity = s.OrbitSensitivity,
                minPitch         = s.MinPitch,
                maxPitch         = s.MaxPitch,
                orbitSmoothing   = s.OrbitSmoothing,
                zoomSpeed        = s.ZoomSpeed,
                zoomSmoothing    = s.ZoomSmoothing,
                minZoomDistance  = s.MinZoomDistance,
                maxZoomDistance  = s.MaxZoomDistance,
                panSensitivity   = s.PanSensitivity,
                panSmoothing     = s.PanSmoothing
            };
        }

        private static CameraSettings CameraSettingsFromDTO(CameraSettingsDTO dto)
        {
            if (dto == null) return new CameraSettings();
            return new CameraSettings
            {
                OrbitSensitivity = dto.orbitSensitivity,
                MinPitch         = dto.minPitch,
                MaxPitch         = dto.maxPitch,
                OrbitSmoothing   = dto.orbitSmoothing,
                ZoomSpeed        = dto.zoomSpeed,
                ZoomSmoothing    = dto.zoomSmoothing,
                MinZoomDistance  = dto.minZoomDistance,
                MaxZoomDistance  = dto.maxZoomDistance,
                PanSensitivity   = dto.panSensitivity,
                PanSmoothing     = dto.panSmoothing
            };
        }

        private static SceneObjectDTO ObjectToDTO(SceneObject obj)
        {
            var t = obj.Transform;
            return new SceneObjectDTO
            {
                id          = obj.Id,
                displayName = obj.DisplayName,
                category    = obj.Category,
                modelPath   = obj.ModelPath,
                isImported  = obj.IsImported,
                importPath  = obj.ImportPath,
                isPawn      = obj.IsPawn,
                metadata = obj.Metadata?.Select(EntryToDTO).ToList() ?? new List<MetadataEntryDTO>(),
                posX   = t?.Position.x ?? 0, posY   = t?.Position.y ?? 0, posZ   = t?.Position.z ?? 0,
                rotX   = t?.Rotation.x ?? 0, rotY   = t?.Rotation.y ?? 0,
                rotZ   = t?.Rotation.z ?? 0, rotW   = t?.Rotation.w ?? 1,
                scaleX = t?.Scale.x    ?? 1, scaleY = t?.Scale.y    ?? 1, scaleZ = t?.Scale.z    ?? 1
            };
        }

        private static MetadataEntryDTO EntryToDTO(MetadataEntry entry)
        {
            var dto = new MetadataEntryDTO { entryType = entry.EntryType };
            switch (entry.EntryValue)
            {
                case NoteValue note:
                    dto.noteText = note.Text;
                    break;
                case TagValue tag:
                    dto.tagId       = tag.Id;
                    dto.tagName     = tag.Name;
                    dto.tagHexColor = tag.HexColor;
                    break;
                case SheetValue:
                    // TODO: populate when sheets are implemented
                    break;
            }
            return dto;
        }

        private static SceneObject ObjectFromDTO(SceneObjectDTO dto)
        {
            return new SceneObject
            {
                Id          = dto.id,
                DisplayName = dto.displayName,
                Category    = dto.category,
                ModelPath   = dto.modelPath,
                IsImported  = dto.isImported,
                ImportPath  = dto.importPath,
                IsPawn      = dto.isPawn,
                Metadata    = dto.metadata?.Select(EntryFromDTO).ToList() ?? new List<MetadataEntry>(),
                Transform   = new TransformModel
                {
                    Position = new Vector3(dto.posX,  dto.posY,  dto.posZ),
                    Rotation = new Quaternion(dto.rotX, dto.rotY, dto.rotZ, dto.rotW),
                    Scale    = new Vector3(dto.scaleX, dto.scaleY, dto.scaleZ)
                }
            };
        }

        private static MetadataEntry EntryFromDTO(MetadataEntryDTO dto)
        {
            MetadataValue value = dto.entryType switch
            {
                "note"  => new NoteValue { Text = dto.noteText },
                "tag"   => new TagValue { Id = dto.tagId, Name = dto.tagName, HexColor = dto.tagHexColor },
                "sheet" => new SheetValue(),
                _       => null
            };
            return new MetadataEntry { EntryType = dto.entryType, EntryValue = value };
        }
    }
}
