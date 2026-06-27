using System;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class CreateTagUseCase
    {
        private readonly TagCollection _tags;

        public CreateTagUseCase(TagCollection tags)
        {
            _tags = tags;
        }

        public Tag Execute(string name, string hexColor = "#FFFFFF")
        {
            var tag = new Tag
            {
                Id       = Guid.NewGuid().ToString(),
                Name     = name,
                HexColor = hexColor
            };
            _tags.Add(tag);
            return tag;
        }
    }
}
