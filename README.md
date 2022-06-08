# MonoGame_GaussianBlur
Demonstrates the use of a Gaussian Blur effect in MonoGame. 

![demo](https://user-images.githubusercontent.com/5315733/172723081-8b4c4003-dbff-4b24-81b7-f5204d3f3c6c.png)

This project is a conversion of the "XNA 4.0 Gaussian Blur Demo" project originally made by dhpoware to MonoGame. The FX effect is slightly changed as well to be compatible with the current MonoGame content builder.

Original resources (and many thanks!):
* http://www.dhpoware.com/demos/xnaGaussianBlur.html
* https://twitter.com/thirtyvirus/status/704772418611638273
* https://pastebin.com/yTAb2SwU

### PS
I am quite new to MonoGame and still learning a lot about the engine itself and everything related to sprites. This is actually the first time I see an effect in action, and still need to grasp what actually is going on, but hey, it's working. 

Since MonoGame resources seem sparse and I appreciate every contribution (such as tutorials and such), I decided to try and get this old XNA code working again in a modern MonoGame project and share the code for people like me, who are searching for good demo projects.

Btw, this project is using "net472" as the target framework, but you should be able to use .net core 3 (or higher) as well by changing the <TargetFramework> in the csproj file.
