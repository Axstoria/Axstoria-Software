namespace Controler.Math
{
    public class Quaternion
    {
        public readonly float x;
        public readonly float y;
        public readonly float z;
        public readonly float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity => new(0f, 0f, 0f, 1f);
    }
}