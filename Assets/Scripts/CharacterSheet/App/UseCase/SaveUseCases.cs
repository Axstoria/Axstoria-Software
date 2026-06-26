using CharacterSheet.Domain;
using Newtonsoft.Json;
using Shared.Domain;
using UnityEngine;

namespace CharacterSheet.App.UseCase
{
    public class SaveUseCases
    {
        private readonly ISaveRepository _saveRepository;

        public SaveUseCases(ISaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public void Execute(Sheet sheet)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(sheet, settings);
            _saveRepository.Save(sheet.Id, json);
        }
    }

    public class LoadUseCases
    {
        private readonly ISaveRepository _saveRepository;

        public LoadUseCases(ISaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public Sheet Execute(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !_saveRepository.Exists(fileName)) {
                return null;
            }

            try {
                string json = _saveRepository.Load(fileName);

                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                return JsonConvert.DeserializeObject<Sheet>(json, settings);
            }
            catch (System.Exception ex) {
                return null;
            }
        }
    }
}