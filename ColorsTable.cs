using System.Drawing;

namespace EnhancedMap
{
    public static class ColorsTable
    {
        public static readonly Color Blue0 = Color.FromArgb(29, 124, 167);
        public static readonly Color Blue1 = Color.FromArgb(45, 142, 184);
        public static readonly Color Blue2 = Color.FromArgb(28, 156, 214);
        public static readonly Color Blue3 = Color.FromArgb(47, 164, 217);
        public static readonly Color Blue4 = Color.FromArgb(83, 180, 223);

        public static readonly Color Black0 = Color.FromArgb(49, 49, 49);
        public static readonly Color Black1 = Color.FromArgb(104, 44, 44);
        public static readonly Color Black2 = Color.FromArgb(89, 59, 59);
        public static readonly Color Black3 = Color.FromArgb(70, 70, 70);
        public static readonly Color Black4 = Color.FromArgb(39, 39, 39);


        public static readonly Color White0 = Color.FromArgb(179, 255, 255, 255);
        public static readonly Color Gray50Perc = Color.FromArgb(50.PercentageToColorComponent(), 0x999999.ToColor());
        public static readonly Color Gray20Perc = Color.FromArgb(20.PercentageToColorComponent(), 0x999999.ToColor());
    }
}