#if DX
namespace Nine.Graphics.Content.DirectX
#else
namespace Nine.Graphics.Content.OpenGL
#endif
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Framework.Runtime;

    public class FontLoader
    {
        private readonly IContentProvider contentProvider;
        private readonly SharpFont.Library freetype;

        public FontLoader(NuGetDependencyResolver nuget, IContentProvider contentProvider = null)
        {
            var sharpFontDependencies = nuget.Dependencies.FirstOrDefault(d => d.Resolved && d.Identity.Name == "SharpFont.Dependencies");
            if (sharpFontDependencies == null) throw new InvalidOperationException("Cannot load SharpFont.Dependencies");

            var freetypePath = Path.Combine(sharpFontDependencies.Path, "bin/msvc9");
            var arch = IntPtr.Size == 8 ? "x64" : "x86";

            Interop.LoadLibrary(Path.Combine(freetypePath, arch, "freetype6.dll"));

            this.freetype = new SharpFont.Library();
            this.contentProvider = contentProvider;
        }

        public void Load(string text)
        {

        }
    }
}
