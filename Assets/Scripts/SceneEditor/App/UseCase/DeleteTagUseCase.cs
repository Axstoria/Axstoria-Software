using MapEditor.Domain;

namespace SceneEditor.App.UseCase
{
    public class DeleteTagUseCase
    {
        private readonly Map _map;

        public DeleteTagUseCase(Map map)
        {
            _map = map;
        }

        public void Execute(string tagId)
        {
            _map.Tags.Remove(tagId);

            foreach (var obj in _map.Objects)
                obj.Tags.Remove(tagId);
            foreach (var token in _map.Tokens)
                token.Tags.Remove(tagId);
            foreach (var structure in _map.Structures)
                structure.Tags.Remove(tagId);
        }
    }
}
