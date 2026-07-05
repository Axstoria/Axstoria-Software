using Grid.Domain;
using MapEditor.App.Command;
using MapEditor.Domain;
using Shared.Domain;

namespace MapEditor.App.UseCase
{
    public class GenerateTerrainUseCase
    {
        private readonly CommandHistory   _history;
        private readonly Grid.Domain.Grid _grid;
        private readonly Map              _map;

        public GenerateTerrainUseCase(CommandHistory history, Grid.Domain.Grid grid, Map map)
        {
            _history = history;
            _grid    = grid;
            _map     = map;
        }

        public void Execute(TerrainLayout terrain, int width, int depth, int thickness, float height, float[] color)
        {
            _history.Record(new GenerateTerrainCommand(terrain, _grid, _map, width, depth, thickness, height, color));
        }
    }
}
