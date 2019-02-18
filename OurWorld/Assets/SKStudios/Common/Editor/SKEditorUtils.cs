using System.Diagnostics;
using System.IO;

namespace SKStudios.Common.Editor {
    public static class SkEditorUtils {
        // @Hack, there's got to be a better way to do this
        public static string GetAssetRoot(string assetName)
        {
            var path = new StackTrace(true).GetFrame(0).GetFileName();
            if (path == null) return string.Empty;

            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            var root = path.Substring(0, path.LastIndexOf(assetName) + assetName.Length + 1);
            return root;
        }
    }
}