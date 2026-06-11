# Twitch integration

## Description

Application integrates with Twitch API to create clips and schedule recurring segments. Integration with Twitch is just an additional feature and is not required by the application in order for it to work. Base features of the application such as creating tournaments and managing matches don't depend on this integration.

## Elements of integration

### Scheduling a recurring segment

Application exposes an endpoint to schedule a recurring segment. A segment is an entry in Twitch database that informs about a planned live transmission. Planning a segment doesn't mean that the transmission will automatically start at the planned time, instead it acts as a calendar entry. Segments can be retrieved using the Twitch API or can be accessed on the Twitch website.

### Creating a clip

Application exposes an endpoint to create a clip. A clip is a video that covers a short part of the live transmission before the clip creation was requested. After clip is created, the url of this newly created clip gets returned and user can edit and then publish the clip.
