using System;
using System.Threading.Tasks;

namespace RedisAppendStreams.Test
{
    public class RedisFixture : DockerFixture
    {
        public RedisFixture()
            : base(dockerUri: new Uri("npipe://./pipe/docker_engine"), containerSpecs: new[] {
                new ContainerSpec("app-logs-test-redis-1", "redis:4", new[] { (16379, 6379) }),
                new ContainerSpec("app-logs-test-redis-2", "redis:4", new[] { (26379, 6379) }, "--slaveof 127.0.0.1 16379")
            })
        { }

        protected override Task AfterStart()
            => Task.Delay(1000);
    }


}
