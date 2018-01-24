# GraphDatabaseDemo
This repository was created for a talk given at SweTugg conference in Stockholm February 2018. It is used to demonstrate how graph data in Neo4j can be handled from C#. It holds some generic methods that might very well be useful as a starting point for other projects.


## Neo4j Editions
The Neo4j server exists in a Community version and an Enterprise version.
The administration can be reached by using the server web interface.
The administration is also packaged into a "desktop bundle" with similar interface but launched as a desktop application.

For demo usage the easiest way is probably to click the default Download button on: https://neo4j.com/download/

That will install the **Neo4j Desktop** and the **Neo4j Enterprise Edition for Developers**.


## Setup
Follow the instructions to download and install Neo4j.
When prompted for a user choose whatever authentication method you prefer.
The authentication is global and used **to login to the desktop panel**.
If everything works you will now have a Neo4j Desktop window.

- Click New Database
- Click Local
- Change suggested name from Database to Movies
- Accept latest version and click Create

This will download the selected driver version and generate an empty database.
When database is available click Start

Every new database will by default use the following values:

    User: neo4j
    Password: neo
    Http port: 7474
    Bolt port: 7687

This can be modified using commands like `neo4j-admin set-initial-password <password>` but for now I suggest you leave them with their default values also specified in the app.config file.


## Load Movies
By running the Neo.Movies console application you will install Movies from the zip file located in Resources folder.
This will give you 8000 movies, 60000 persons and 150000 relations to play with. The generation will typically take ~20 minutes. You might feel this is slow but it is actually quite fast. An average record operation takes ~0.5ms which is about ten times faster than most RDBMS. Imports can of course be bulk processed in even faster ways, but that is outside the scope of this demo.


## Experiment with Cypher language
The easisest way to play around with data is by using the provided Neo4j client.
If you installed the Desktop edition you will have a Windows icon to start it.
Here are some suggestions to start with:

```
MATCH (m:Movie)
WHERE m.originalLanguage='sv'
RETURN m.title

MATCH (p:Person) - [:ACTED_IN] -> (m:Movie)
WHERE m.title STARTS WITH 'C'
RETURN m.title, p.name

MATCH (p:Person) - [:ACTED_IN] -> (m:Movie)
RETURN m.title, collect(p.name)

MATCH (p:Person) - [:ACTED_IN] -> (m:Movie)
WITH
    {name: p.name, born: p.born} AS PersonStruct,
    {name: m.title, tag: m.tagline} AS MovieStruct
RETURN MovieStruct AS Movie, collect(PersonStruct) AS Actors
LIMIT 10
```

## Code

### Neo.Movies.Program
Some examples is provided of how to do some common tasks from code:
- Retrieve typed objects
- Retrieve custom data from custom query
- Add node
- Delete node

### Neo.Movies.Entities
This is just a simple representation of the movie structures as POCO objects.
MovieContainer is used to deserialize the json content found in Neo.Movies/Resources/MovieContainer.zip and import it into Neo4j database.

### NeoDriverRepository
This uses the official C# Neo4j.Driver to manipulate Neo4j data.
The provided generic methods might be helpful to reuse in other projects as they perform some typical tasks like adding generic nodes, relations and indexes.
This repository mainly uses plain Cypher queries.

### NeoClientRepository
This implements almost the same methods using another driver Neo4jClient created by the community.
The ambition of that driver is to abstract Cypher by using more ".Net style methods".
The downside is that it ends up being almost more complicated to use.
This client is also a bit behind the official one (like no implementation of Bolt protocol).
So in the end I found I mostly prefer the official Neo4j.Driver.