using System;
using System.Collections.Generic;
using App.Ports;
using Domain;
using DomainGrid  = Domain.Grid;
using DomainVec3  = Domain.Math.Vector3;
using DomainQuat  = Domain.Math.Quaternion;

namespace Infrastructure.Persistence
{
    public class JsonMapSerializer : IMapSerializer
    {
        public string Serialize(Map map)
        {
            return UnityEngine.JsonUtility.ToJson(ToDTO(map), true);
        }

        public Map Deserialize(string json)
        {
            return FromDTO(UnityEngine.JsonUtility.FromJson<MapDataDTO>(json));
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
                objects = new List<SceneObjectDTO>()
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
                TerrainLayout = TerrainFromDTO(dto.terrain)
            };

            foreach (var objDTO in dto.objects)
                map.Objects.Add(ObjectFromDTO(objDTO));

            return map;
        }

        // ── TerrainLayout ↔ DTO ───────────────────────────────────────────────

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

        // ── SceneObject ↔ DTO ─────────────────────────────────────────────────

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
                posX   = t?.Position.x ?? 0, posY   = t?.Position.y ?? 0, posZ   = t?.Position.z ?? 0,
                rotX   = t?.Rotation.x ?? 0, rotY   = t?.Rotation.y ?? 0,
                rotZ   = t?.Rotation.z ?? 0, rotW   = t?.Rotation.w ?? 1,
                scaleX = t?.Scale.x    ?? 1, scaleY = t?.Scale.y    ?? 1, scaleZ = t?.Scale.z    ?? 1
            };
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
                Transform   = new TransformModel
                {
                    Position = new DomainVec3(dto.posX,  dto.posY,  dto.posZ),
                    Rotation = new DomainQuat(dto.rotX,  dto.rotY,  dto.rotZ, dto.rotW),
                    Scale    = new DomainVec3(dto.scaleX, dto.scaleY, dto.scaleZ)
                }
            };
        }
    }
}
