using AssetImporter.AssetImporter.App;
using CharacterSheet.App.DTO;
using CharacterSheet.Domain;

namespace CharacterSheet.App.UseCase
{
    public class UpdateAppearanceUseCase
    {
        public void Execute(IAppearanceTarget target, AppearanceDTO dto) {
            if (dto.HasBorder.HasValue)
                target.HasBorder       = dto.HasBorder.Value;
            if (dto.BorderThickness.HasValue)
                target.BorderThickness = dto.BorderThickness.Value;
            if (dto.BorderColor.HasValue)
                target.BorderColor     = dto.BorderColor.Value;
            if (dto.BackgroundColor.HasValue)
                target.BackgroundColor = dto.BackgroundColor.Value;
            //widget.OnAppearanceChanged?.Invoke();
        }
    }

    public class UpdateBackgroundUseCase
    {
        private readonly IImageImportService _imageImporter;
        
        public UpdateBackgroundUseCase(IImageImportService imageImporter)
        {
            _imageImporter = imageImporter;
        }

        public void Execute(IAppearanceTarget target, string parentContainer)
        {
            var newImagePath = _imageImporter.ImportImageFromDisk(parentContainer);
            
            if (string.IsNullOrEmpty(newImagePath)) return;
            target.BackgroundImagePath = newImagePath;
        }
    }
}