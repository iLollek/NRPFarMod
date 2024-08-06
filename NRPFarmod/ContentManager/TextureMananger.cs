using MelonLoader;
using NRPFarmod.UIHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace NRPFarmod.ContentManager {

    public static class TextureMananger {

        #region AboutUI




        #region AboutUI Position



        #endregion
        #endregion

        #region Create Texture
        /// <summary>
        /// Lädt die Texture
        /// </summary>
        /// <param name="tmp"></param>
        /// <param name="data"></param>
        public static void CreateTexture(ref Texture2D? tmp, byte[] data) {

            try {
                Color32 transparent = new Color(0, 0, 0, 0);
                Color32 currentColor = Color.blue;
                var enumerator = LoadValuesFromByte(data);

                enumerator.MoveNext();

                uint width = enumerator.Current; enumerator.MoveNext();
                uint height = enumerator.Current; enumerator.MoveNext();

                //MelonLogger.Msg($"Texture: \u001b[32m{width}\u001b[0mx\u001b[32m{height}\u001b[0m");

                tmp = new Texture2D((int)width, (int)height);

                //Clear
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        tmp.SetPixel(x, y, transparent);

                int index = 0;

                do {
                    var value = enumerator.Current;
                    var colors = ShiftOut(value);
                    currentColor = new Color32(colors.Item1, colors.Item2, colors.Item3, colors.Item4);

                    int x = index % (int)width;
                    int y = (int)height - 1 - (index / (int)width);

                    tmp.SetPixel(x, y, currentColor);

                    index++;
                } while (enumerator.MoveNext());
                tmp.Apply();
                //MelonLogger.Msg($"Texture: \u001b[32mok\u001b[0m");
            } catch (Exception) {
                MelonLogger.Msg($"Texture: \u001b[31mERROR\u001b[0m");
            }

        }

        /// <summary>
        /// Ließt die Color Werte aus dem Bytearray aus
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerator<uint> LoadValuesFromByte(byte[] obj) {
            using (var stream = new MemoryStream(obj))
            using (var reader = new BinaryReader(stream)) {
                while (reader.BaseStream.Position < reader.BaseStream.Length) {
                    uint val = reader.ReadUInt32();
                    yield return val;
                }
            }
        }

        /// <summary>
        /// Bitmanipulation
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        public static (byte, byte, byte, byte) ShiftOut(uint blob) {
            byte a = (byte)(blob & 0xFF);
            byte b = (byte)((blob >> 8) & 0xFF);
            byte c = (byte)((blob >> 16) & 0xFF);
            byte d = (byte)((blob >> 24) & 0xFF);
            return (a, b, c, d);
        }
        #endregion
    }
}
