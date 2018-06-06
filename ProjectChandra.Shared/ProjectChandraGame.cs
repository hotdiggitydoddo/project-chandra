using Nez;
using ProjectChandra.Shared.Scenes;

namespace ProjectChandra.Shared
{
    public class ProjectChandraGame : Core
    {
        protected override void Initialize()
        {
            base.Initialize();
            scene = new TestScene();
        }
    }
}
