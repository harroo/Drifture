
## SETUP

on ur server, install the `DriftureServer/` directory into ur assets
and on client to the `DriftureClient/` one

## USAGE

### SETUP IN UNITY

in unity u need to add `DrifturePlayerMonitor` to ur player, this will
update the players position to the server side so the server can calculate
nearest players to entities for control management

u can change this system if you want

### INTERFACING WITH UR CODE

#### SUBMANAGER

the submanager basicly controls who is controlling what entity;
on the server it has a check method that u should call every minute or 2, it will run checks
for who is controlling the entity, on the on both client and serevr it has a check u will need to do
to like check if there is messages ready to be send, and send them, here are the examples:::::

using this ur gona need to have a system in ur own code that checks the submanager
the namespace for the submanager is `using Drifture`
```cs
//there is msgs cached up in the send queue
if (Submanager.SendCount() != 0) {

    //get the data (x, y, z of player pos in byte[])
    byte[] sendData = Submanager.PopSendQueue();

    // >>> send it somehow
}

//when its recv on the server, do this in the server
Submanager.UpdatePlayerPos(recvData);

//and make sure u do this every minute or 2
//this will make it check all the player positions and update control
Submanager.RunChecks();
```

then on the server you should do this stuff
```cs
//there is msgs cached up in the send queue
if (Submanager.SendCount() != 0) {

    //get the data (ulong, string of entityid and new controller in byte[])
    byte[] sendData = Submanager.PopSendQueue();

    // >>> send to all clients somehow
}

//then on client when this is recv, do this
Submanager.UpdateControl(recvData);

```

#### DRIFTUREMANAGER

there is a value in here called `DriftureManager.thisName` it is used as the
local clients `playerNameId`, set it to whatever you want to id ur players as
it a `string`

then there are some functions u can configure
```cs
//to kill/despawn and entity
DriftureManager.DespawnEntity = (ulong entityId) => {

    //send the data to the server, the server should relay it
};
//to interact with an entity
DriftureManager.InteractEntity = (ulong entityId, object sender) => {

    //send the data to the server, the server should relay it
};
//to attack an entity
DriftureManager.AttackEntity = (ulong entityId, int damage, object sender) => {

    //send the data to the server, the server should relay it
};
```
the serevr shouild relay all of these back to the clients, after optional processing if it needs to do some stuff

#### ENTITYMANAGER

configuring the entity managre
u will need `using Drifture`

##### CLIENT

so making this work on ur client is easy, just add methods linking to the managre like this
##### `POSITION` sending
```cs
//this is called once per 0.1 seconds by every entity
EntityManager.UpdateEntityPosition = (ulong entityId, Vector3 pos, Quaternion rot) => {

    //send the pos and rot to the server, prefrebly over udp
};
//this is called once per second by every entity
EntityManager.EnsureEntityPosition = (ulong entityId, Vector3 pos, Quaternion rot) => {

    //send the pos and rot to the server, over tcp, this is to ensure position syncing
    //its not super importent but its an option for ur game
};
```
##### `POSITION` recving
```cs
//recv entity pos update
EntityManager.UpdateTransform(entityId, entityPosition, entityRotation);
```
##### `METADATA` sending
```cs
//called by the entity scripts themselves, they want to save there data on teh server
EntityManager.SyncEntityMetaData = (ulong entityId, byte[] metaData) => {

    //send the meta data to the server
};
```
##### `METADATA` recving
```cs
//recv the update data
EntityManager.UpdateMetaData(entityId, metaData);
```
##### `DESPAWNING`
```cs
//u will recv from the server saying to desapawn and entity\
//this is when an entity dies
EntityManager.DespawnEntity(entityId);
```
##### `INTERACTION`
```cs
//u will recv from the server to interact with a entity
//you then call with the data:
EntityManager.InteractEntity(entityId, senderObject);
//u choose what sender is
```
##### `ATTACK`
```cs
//u will recv from the server to attack with a entity
//you then call with the data:
EntityManager.AttackEntity(entityId, senderObject);
//u choose what sender is
```

##### SERVER

then on the server this is how u handle all of those things
u gona recv the messages this is how u handle em
##### `POSITION`
```cs
//recv data about entity pos update, do this
EntityManager.UpdateTransform(entityId, entityPosition, entityRotation);
//then relay it to all the other clients
```
##### `METADATA`
```cs
//recv meta data update info
EntityManager.UpdateMetaData(entityId, metaData);
//then relay it to all the other clients to
```

### DRIFTUREMANAGER

this thing will give u functions to control stuff
so there are functions to create and delete entitys, and there are functions
to attack and interact with entitys, use this thing in ur behaviours and stuff
to interact and etc, u will need `using Drifture`

heres a list of all the actions u can call and what they do
```cs
//deletes/despawns an entity
DriftureManager.DespawnEntity (ulong entityId);
//interacts with an entity
DriftureManager.InteractEntity (ulong entityId, object sender);
//attacks an entity
DriftureManager.AttackEntity (ulong entityId, int damage, object sender);
```
