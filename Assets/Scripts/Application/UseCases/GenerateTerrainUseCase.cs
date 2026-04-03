using App.Commands;
using Domain;

namespace App.UseCases
{
    public class GenerateTerrainUseCase
    {
        private readonly CommandHistory _history;

        public GenerateTerrainUseCase(CommandHistory history)
        {
            _history = history;
        }

        public void Execute(TerrainLayout terrain, int width, int depth, int thickness, float height, float[] color)
        {
            _history.Record(new GenerateTerrainCommand(terrain, width, depth, thickness, height, color));
        }
    }
}
