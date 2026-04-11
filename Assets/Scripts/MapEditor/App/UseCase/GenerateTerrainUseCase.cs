using App.Commands;
using Domain;

namespace App.UseCases
{
    /// <summary>
    /// Use case for generating terrain based on a specified layout and parameters.
    /// </summary>
    public class GenerateTerrainUseCase
    {
        private readonly CommandHistory _history;
        private readonly Grid           _grid;
        private readonly Map            _map;

        public GenerateTerrainUseCase(CommandHistory history, Grid grid, Map map)
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
