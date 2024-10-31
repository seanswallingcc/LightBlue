using System.Diagnostics;
using System.Reflection;
using LightBlue.Setup;

namespace LightBlue.Infrastructure
{
    public class HostRunner
    {
        public void Run(
            string workerRoleAssembly,
            string configurationPath,
            string roleName,
            bool useHostedStorage)
        {
            LightBlueConfiguration.SetAsLightBlue(
                configurationPath: configurationPath,
                roleName: roleName,
                lightBlueHostType: LightBlueHostType.Direct,
                useHostedStorage: useHostedStorage);

            var assembly = Assembly.LoadFrom(workerRoleAssembly);
            assembly.EntryPoint.Invoke(null, new object[] { null });
        }
    }

    //Runs a specified Library using a specified Host, within a LB Host
    public class LibraryRunner
    {
        public void Run(
           string hostAssembly,
           string libraryAssembly,
           string configurationPath,
           string roleName,
           bool useHostedStorage)
        {
            LightBlueConfiguration.SetAsLightBlue(
                configurationPath: configurationPath,
                roleName: roleName,
                lightBlueHostType: LightBlueHostType.Direct,
                useHostedStorage: useHostedStorage);
            Trace.TraceInformation($"Library: {libraryAssembly}");
            Trace.TraceInformation($"Host: {hostAssembly}");
            var assembly = Assembly.LoadFrom(hostAssembly);
            assembly.EntryPoint.Invoke(null, new object[] { new string[] { "-p", libraryAssembly } });
        }
    }
}