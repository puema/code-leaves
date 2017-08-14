using Zenject;

namespace Core
{
    public class BindingManager : MonoInstaller<BindingManager>
    {
        public override void InstallBindings()
        {
            Container.Bind<ApplicationManager>().AsSingle();
        }
    }
}