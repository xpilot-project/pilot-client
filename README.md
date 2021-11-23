# Archived

This repository is archived. No further updates will be made available for this version of xPilot. 

Users are encouraged to upgrade to xPilot 2.0, which brings native support to macOS and Linux. 

You can download and follow the progress on the new version here: https://github.com/xpilot-project/xpilot

Happy flying! ✈️

-----

# xPilot - Pilot Client

xPilot is an intuitive and easy to use X-Plane pilot client for the VATSIM network. The pilot client is a Windows c# WinForms application that utilizes NetMQ to transport data to and from the [X-Plane Plugin](https://github.com/xpilot-project/Plugin) through the use of TCP socket communication.

### Prerequisites 

* VisualStudio v16 (or newer)
* .NET 4.7.2 Framework (or newer)
* [FSD-Connector](https://github.com/xpilot-project/FSD-Connector)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

`git clone https://github.com/xpilot-project/Pilot-Client.git`

* Open the solution `XPilot.PilotClient.sln` in VisualStudio
* Build from within VisualStudio

## Contributing

Please read the [Contribution Guide](CONTRIBUTING.md) for details on how to contribute to the project.

## Testing

Development builds of the xPilot client **cannot** be connected on the live VATSIM network. If you are interested in contributing to the xPilot project, please contact Justin Shannon in the [xPilot Discord](https://vats.im/xpilot-discord) for access to a development FSD server.

## Versioning

xPilot uses [Semantic Versioning](http://semver.org/). See [tags](https://github.com/xpilot-project/Pilot-Client/tags) on this repository for published versions.

## License

This project is licensed under the [GPLv3 License](LICENSE).

## Acknowledgments

* [GeoVR](https://github.com/macaba/GeoVR) Audio for VATSIM Client Library
* [XPlaneConnector](https://github.com/MaxFerretti/XPlaneConnector) Read X-Plane datarefs via UDP
* [Appccelerate](http://www.appccelerate.com/) EventBroker
* [NetMQ](https://github.com/zeromq/netmq) Messaging Library
* [NAudio](https://github.com/naudio/NAudio)
* [Ninject](https://github.com/ninject/Ninject) Dependecy Injector
* [SharpDX](http://sharpdx.org/)
* Application icon created by [Freepik](https://www.flaticon.com/authors/freepik)
