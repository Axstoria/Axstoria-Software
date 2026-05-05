using Grid.Domain;
using MapEditor.Domain;
using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class DeleteObjectUseCase
    {
        private readonly Map              _map;
        private readonly Grid.Domain.Grid _grid;
        private readonly CommandHistory   _history;

        public DeleteObjectUseCase(Map map, Grid.Domain.Grid grid, CommandHistory history)
        {
            _map     = map;
            _grid    = grid;
            _history = history;
        }

        public void Execute(SceneObject obj)
        {
            _history.Record(new DeleteObjectCommand(_map, _grid, obj));
        }
    }
}
