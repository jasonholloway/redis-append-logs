using Docker.DotNet;
using Docker.DotNet.Models;
using RedisAppendLogs.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RedisAppendLogs.Test
{
    public struct ContainerSpec
    {
        public readonly string Image;
        public readonly string Name;
        public readonly string Args;
        public readonly (int, int)[] Ports;
        
        public ContainerSpec(string name, string image, (int, int)[] ports, string args = null)
        {
            Name = name;
            Image = image;
            Ports = ports;
            Args = args;
        }
    }
    

    public abstract class DockerFixture : IAsyncLifetime
    {
        readonly Uri _dockerUri;
        readonly (ContainerSpec, string id)[] _conts;

        readonly DockerClient _docker;

        public DockerFixture(Uri dockerUri, ContainerSpec[] containerSpecs)
        {
            _dockerUri = dockerUri;
            _conts = containerSpecs.Map(s => (s, (string)null)).ToArray();

            _docker = new DockerClientConfiguration(_dockerUri)
                            .CreateClient();
        }



        async Task StartContainer((ContainerSpec, string id) cont)
        {
            var (spec, id) = cont;
            
            await KillContainer(cont);
            
            await PullImage(spec.Image);

            var contParams = new CreateContainerParameters
            {
                Name = spec.Name,
                Image = spec.Image,
                ExposedPorts = spec.Ports.ToDictionary(m => m.Item2.ToString(), m => new EmptyStruct()),
                HostConfig = new HostConfig
                {
                    PortBindings = spec.Ports.ToDictionary(
                                                m => m.Item2.ToString(), 
                                                m => new PortBinding { HostIP = "0.0.0.0", HostPort = m.Item1.ToString() }.WrapAsList()
                                                )                    
                }
            };

            var response = await _docker.Containers.CreateContainerAsync(contParams);
            var containerId = response.ID;
            
            var success = await _docker.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

            if(success) cont.id = containerId;
        }



        async Task PullImage(string imageName)
        {
            var found = await _docker.Images.ListImagesAsync(new ImagesListParameters { MatchName = imageName })
                                .Map(r => r.Any());

            if (!found)
            {
                await _docker.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = imageName }, null, new DummyProgress<JSONMessage>());
            }
        }



        async Task KillContainer((ContainerSpec spec, string id) cont)
        {            
            await _docker.Containers.RemoveContainerAsync(cont.spec.Name, new ContainerRemoveParameters { Force = true })
                        .IgnoreException<DockerContainerNotFoundException>();

            cont.id = null;
        }



        Task StartContainers()
            => _conts.Map(s => StartContainer(s)).WhenAll();


        Task KillContainers()
            => _conts.Map(s => KillContainer(s)).WhenAll();


        Task IAsyncLifetime.InitializeAsync()
            => StartContainers()
                .ContinueWith(_ => AfterStart())
                .Unwrap();

        Task IAsyncLifetime.DisposeAsync()
            => KillContainers();


        protected virtual Task AfterStart() => Task.CompletedTask;
        

        class DummyProgress<T> : IProgress<T>
        {
            public void Report(T value)
            { }
        }

    }

}
