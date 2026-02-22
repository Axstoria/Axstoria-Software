namespace Domain.Math
{
    public class Vector3
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 zero => new(0f, 0f, 0f);
        public static Vector3 one  => new(1f, 1f, 1f);
    }
}