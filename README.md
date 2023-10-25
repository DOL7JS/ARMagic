# ARMagic
The application was created as a diploma thesis at the University of Pardubice called [Vývoj ukázkové aplikace pro rozšířenou realitu](https://github.com/DOL7JS/ARMagic/blob/main/DolejsP_VyvojUkazkove_JP_2023.pdf) (Development of a sample application for augmented reality). The application demonstrates the possibilities of augmented reality offered by ARCore technology. 
Along with the application, a custom data storage server was also designed and presented. This application is described in detail in the thesis: https://hdl.handle.net/10195/82362.

This application is divided into four parts, each focusing on one of the possibilities of augmented reality. Due to the use of ARCore technology to work with augmented reality on the Android operating system, these options were limited compared to ARKit. Additionally, when using ARKit, it would be possible to perform object recognition or body recognition. The main capabilities that ARCore offers and have been used in the app include marker,
face and horizontal surface recognition. To save storage on the device, a server was also designed to store data in the form of 3D models, their configurations and videos. To display the 3D models, the application uses models in GLB format.
# Stack
- Unity Engine
- C#
- ARCore
- XAMPP

# Get started
Whole setting process is described in thesis mentioned earlier. This will be just short description.
1. Set up server
2. Upload glb models, markers, videos
3. Set access to server from outside world
4. Install AR Magic on Android device
5. Connect to same WiFi as server is running
6. Test connection to server
7. Download models and have fun!


# Sample data
In folder [Sample folder](https://github.com/DOL7JS/ARMagic/tree/main/Sample_data) are markers, videos, configurations and models, that can be used in app. Of course you can use any other model or marker you want. Videos, models and configurations are on server meanwhile you have to have marker on your device storage.

# Diagrams
| Activity diagram  | Use case diagram |
| ------------- | ------------- |
![](https://github.com/DOL7JS/ARMagic/assets/53859920/9e26a345-6b24-42f6-8047-02905ef98c84)  |  ![](https://github.com/DOL7JS/ARMagic/assets/53859920/4b64cded-cfea-466b-a0df-4f2087772ee4)

# Model converter
For this app was also created app [Model converter](https://github.com/DOL7JS/ARMagic/tree/main/ModelConverter_app) for Windows. In this app you can set size, rotation and position of model, that will be added to scene in mobile app. You have to import model, set size, rotation, position and then export it to server or locally.
This application is described in detail in the thesis.

<p align="center">
  <img src='https://github.com/DOL7JS/ARMagic/assets/53859920/2ae72d00-4373-40e8-94b6-1d25becb92dd' width='600'>
</p>

# App preview
| AR Marker  | AR Face | AR Video | AR Area |
| ------------- | ------------- | ------------- | ------------- |
| [![AR Marker](https://img.youtube.com/vi/4SPQQWkHrx8/0.jpg)](https://www.youtube.com/watch?v=4SPQQWkHrx8)  | [![AR Marker](https://img.youtube.com/vi/YmFpOwudC7A/0.jpg)](https://www.youtube.com/watch?v=YmFpOwudC7A)|[![AR Marker](https://img.youtube.com/vi/m0jfNKlpd1M/0.jpg)](https://www.youtube.com/watch?v=m0jfNKlpd1M)  | [![AR Marker](https://img.youtube.com/vi/HxVmF_iuEi4/0.jpg)](https://www.youtube.com/watch?v=HxVmF_iuEi4)|
