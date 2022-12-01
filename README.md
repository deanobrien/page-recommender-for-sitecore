![Sitecore Page Recommender](https://deanobrien.uk/wp-content/uploads/2022/11/recommender-768x329.jpg)
# Sitecore Page Recommender
This repository is the companion code to go along side my series of blog posts, that look at my experiences setting up a [page recommendation service for Sitecore](https://deanobrien.uk/sitecore-page-recommender/). 

This is not by any means production ready code. I have pulled the various sections of code from a larger project, so it may need some minor bug fixes. It should however give a clear example of the code required to utilise the Cortex Engine to train and call a Machine Learning model.

I found the Alistair Deneys repo [Cortex Processing Demo](https://github.com/adeneys/cortex-processing-demo) very useful and took alot of inspiration from that.

# How to Deploy

## Deploy the xConnect model to the xConnect server

-   Copy the JSON file ([PageCollectionModel, 1.0.json](https://github.com/deanobrien/page-recommender-for-sitecore/blob/main/src/XConnect/DeanOBrien.XConnect/Models/PageCollectionModel%2C%201.0.json "PageCollectionModel, 1.0.json")) to the  `\Add_Data\Models`  folder on the xConnect server.
-   Restart the xConnect server.

## Deploy the xConnect model to the Cortex Processing Engine

-   Copy the DLL file (DeanOBrien.XConnect.ProcessingEngine) from the output folder of the that project to the Processing Engine folder.

## Deploy the Processing Engine extensions to the Processing Engine

-   Copy the  `App_Data`  folder from the Processing Engine folder to the Processing Engine.
- Restart Processing Engine
## Deploy the Page Recommender Feature to Sitecore Instance

- Copy the  DLL file (DeanOBrien.Feature.PageRecommender) from the output folder of that project to the  `Bin` directory of sitecore instance
- Set up page events
- Set up view to trigger page events
- Add scheduled task to call GeneratePageRecommendationsTask periodically
