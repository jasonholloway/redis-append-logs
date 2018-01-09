# redis-entity-logs
C# client and Lua script combo for storing per-entity logs in Redis 


REDIS DESIGN
-------------------------------------

Lua script will take:
- Key
- Appendage
- Last-Known-Length
	
It will guarantee that:
- writes only happen if the given Last-Known-Length is correct
		
C# client will:
- try to execute script by hash
	- upload script if missing
	- execute script
- return handle to stream
	
TESTING:
- The Lua script will be integrated with the client - ie the client will upload the script
- So, we can just test the client in its target language, C#
- But then the tests need some control over Redis, maybe via docker
- So, C# tests must load dockerized Redis
		
If an LKL is wrong, then the client must re-read. In the future maybe the server can serve the extra data back, but for now...
