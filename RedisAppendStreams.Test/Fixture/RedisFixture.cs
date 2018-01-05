using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RedisAppendStreams.Test
{

    public class RedisFixture : IAsyncLifetime
    {
        readonly string _redisImageName = "redis:4";
        readonly string _redisContainerName = "append-streams-test-redis";
        readonly Uri _dockerUri = new Uri("npipe://./pipe/docker_engine");
        readonly DockerClient _docker;

        string _containerId;

        public RedisFixture()
        {
            _docker = new DockerClientConfiguration(_dockerUri)
                            .CreateClient();
        }

        async Task StartRedis()
        {
            if (_containerId != null) throw new InvalidOperationException("Already have containerId!");

            await _docker.Containers.RemoveContainerAsync(_redisContainerName, new ContainerRemoveParameters { Force = true })
                        .IgnoreException<DockerContainerNotFoundException>();

            await PullRedisImage();

            var contParams = new CreateContainerParameters
            {
                Image = _redisImageName,
                Name = _redisContainerName,
                ExposedPorts = new Dictionary<string, EmptyStruct> {
                    { "6379", new EmptyStruct() }
                },
                HostConfig = new HostConfig {
                    PortBindings = new Dictionary<string, IList<PortBinding>> {
                        { "6379", new[] { new PortBinding { HostIP = "0.0.0.0", HostPort = "6379" } } }
                    }
                }
            };

            _containerId = await _docker.Containers.CreateContainerAsync(contParams)
                                        .Map(r => r.ID);

            var success = await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        async Task PullRedisImage()
        {
            var found = await _docker.Images.ListImagesAsync(new ImagesListParameters { MatchName = _redisImageName })
                                .Map(r => r.Any());

            if (!found)
            {
                await _docker.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = _redisImageName }, null, new DummyProgress<JSONMessage>());
            }
        }


        async Task KillRedis()
        {
            var contId = _containerId;
            _containerId = null;

            await _docker.Containers.RemoveContainerAsync(contId, new ContainerRemoveParameters { Force = true });
        }

        Task IAsyncLifetime.InitializeAsync()
            => StartRedis();

        Task IAsyncLifetime.DisposeAsync()
            => KillRedis();
    }


    class DummyProgress<T> : IProgress<T>
    {
        public void Report(T value)
        { }
    }

}
