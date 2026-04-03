using App.Commands;
using Domain;

namespace App.UseCases
{
    public class DeleteObjectUseCase
    {
        private readonly Map _map;
        private readonly Grid _grid;
        private readonly CommandHistory _history;

        public DeleteObjectUseCase(Map map, Grid grid, CommandHistory history)
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
