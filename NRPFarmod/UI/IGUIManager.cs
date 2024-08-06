using System.Collections;
namespace NRPFarmod.UI
{
    public interface IGUIManager
    {
        public void DrawUI();
        public IEnumerator LoadTextures();
        public void FirstInit();
    }
}
