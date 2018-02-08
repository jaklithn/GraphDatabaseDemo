# Graph Database Demo
This repository was created for a talk given at SweTugg conference in Stockholm February 2018. It is used to demonstrate how graph data in Neo4j can be handled from C#. The project holds some generic methods that might very well be useful as a starting point for other projects.
The presentation slides can be downloaded here: [GraphDatabases.pptx](./slides/GraphDatabases.pptx)


## Neo4j Editions
The Neo4j server exists in a Community version and an Enterprise version.
The administration can be reached by using the server web interface.
The administration is also packaged into a "desktop bundle" with similar interface but launched as a desktop application.
For demo usage the easiest way is probably to click the default Download button on: https://neo4j.com/download/
That will install the **"Neo4j Server Enterprise Edition for Developers"** and the **"Neo4j Desktop"** which is a convenient tool to interact with your databases.


## Neo4j Setup
_(Disclaimer: These instructions were made for Server version 3.3.2 and Desktop version 1.0.11. Future versions might very well change the setup steps.)_
Follow the instructions to download and install Neo4j.
When prompted for a user you can choose whatever authentication method you prefer. Please note that this is only the user of the Desktop Panel and has nothing to do with the database user.
If everything works well you will now have a Neo4j Desktop window.

- Click New Database
- Click Local
- Change suggested name from Database to Movies
- Accept to use the latest version and click Create

This will download the selected driver version and use it to generate an empty database.
Every new database will by default use the following values:

    User: neo4j
    Password: neo4j
    Http port: 7474
    Bolt port: 7687

- When database is available click Start
- When Start is performed (indicating a Stop button) you click Manage button which will open a database management window.
- Click Open Browser to interact with the database. This will open a login page where default values for Host and admin user is provided.
- You enter the initial default password: neo4j
- On first login you are asked to set the admin password. The demo application will use Password: neo456
If you decide somethong else you need to edit the app.config file accordingly.

If you later would like to change the password it is done with a command:

	CALL dbms.security.changePassword('neo456')


## Load Movies
By running the Movies.Neo console application you will install Movies from the zip file located in Resources folder.
This will give you 8000 movies, 60000 persons and 150000 relations to play with. The generation will typically take ~15 minutes. You might feel this is slow but it is actually quite fast. An average record operation takes ~0.4ms which is about ten times faster than most RDBMS. There are several techniques to bulk process imports making it much faster, but that is outside the scope of this demo.


## Experiment with Cypher language
The easisest way to play around with data is by using the provided Neo4j browser window.
Here are some suggestions of queries to start with:

```
MATCH (m:Movie)
WHERE m.originalLanguage='sv'
RETURN m.title

MATCH (p:Person)-[r:ACTED_IN]->(m:Movie)
WHERE m.title="Star Wars: The Last Jedi"
RETURN r.order, r.role, p.name
ORDER BY r.order

MATCH (p:Person)-[ACTED_IN]->(m:Movie)
WHERE m.title STARTS WITH 'C'
RETURN m.title, p.name

MATCH (p:Person)-[ACTED_IN]->(m:Movie)
RETURN m.title, collect(p.name)

MATCH (p:Person)-[ACTED_IN]->(m:Movie)
WITH
    {name: p.name, born: p.born} AS PersonStruct,
    {name: m.title, tag: m.tagline} AS MovieStruct
RETURN MovieStruct AS Movie, collect(PersonStruct) AS Actors
LIMIT 10

MATCH (p:Person)-[r]->(m:Movie)
WHERE p.name="Clint Eastwood"
RETURN p.name, m.releaseDate, m.title, type(r), r.order
ORDER BY m.releaseDate, type(r)
```

For further instructions read the Cypher documentation:
https://neo4j.com/docs/developer-manual/current/get-started/cypher/



## Code
The demo mainly uses these 3 projects: Movies, Neo and Movies.Neo.
The other projects are more experimental (Movies.Cosmos tries to add the Movie domain to Microsoft Azure CosmosDB graph server, Genealogy is an attempt to import an alternative data domain into Neo4j).

### Movies
This project is responsible for defining the movie structures as POCO objects.
MovieParser is used to deserialize the json content found in Resources/MovieContainer.zip into a MovieContainer.

### Neo
Base classes to work with Neo4j database.

- NeoDriverRepository
This class uses the official C# Neo4j.Driver to manipulate Neo4j data.
Tip: Some of the provided generic methods might be helpful to reuse in other projects as they perform some typical tasks like adding generic nodes, relations and indexes.
This repository mainly uses plain Cypher queries.

- NeoClientRepository
I made this repository just as a comparison. It implements almost the same methods using another driver called Neo4jClient which is created by the community.
The ambition of that driver is to abstract Cypher language by using more ".Net style methods" with lambda syntax. When starting off I really liked this ambition. But the downside is that it ends up being almost more complicated to use. Some issues I never managed to do with this client. This client is also a bit behind the official one. So after careful consideration I decided I personally prefer the official Neo4j.Driver.

### Movies.Neo
When you run this console application you get a question on data generation. On first run you should hit Y to accept a generation of data. As mentioned this will take some time, so please be patient. On subsequent runs you can hit any other key which will step you through some provided demo calls. They will show you how to do some common tasks from code:
- Retrieve typed objects
- Retrieve custom data from custom query
- Add node
- Delete node


### Movies.Cosmos
This console application is similar to Movies.Neo. It is added as a quick attempt to use CosmosDB to perform the same tasks. When you are familiar with the graph concept you will probably be able to grasp the approach. My preliminary impression is that a lot of things works the same. In CosmosDB nodes are called Vertex and relations are called Edges.
To start with CosmosDB you need to have an Azure account and create a CosmosDB database.
I suggest you follow instructions from here: https://docs.microsoft.com/en-us/azure/cosmos-db/create-graph-dotnet
When you have a working CosmosDB database you need to add your endPoint and authenticationKey to the app.config file in Movies.Cosmos.

**Disclaimer:
The Cosmos Movie example is currently NOT working! Frankly I got a bit frustrated when things didn't work as I expected. It is a bit confusing when CosmosDB is created by Microsoft and Gremlin language is governed by an external part. The documentation was not enough to solve my issues.**

### Genealogy
Basic structure for genealogy data originally created from standardized GEDCOM files.
PersonParser loads data from Resources/PersonContainer.zip

### Genealogy.Neo
Load genealogy data into Neo4j database.
Please ensure that you have the intended database started in your Neo4j Desktop Client before you import! If you previously imported Movie data with the other console application you might still have that database active, which will cause all movie data to disappear.


## Contact
If you want to contact me for further discussions you can find me here:
LinkedIn: https://www.linkedin.com/in/jakoblithner/
StackOverflow: https://stackoverflow.com/users/1122813/jakob-lithner
Email: jakob.lithner@squeed.com
