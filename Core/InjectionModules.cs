/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2020 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using Vatsim.Xpilot.AudioForVatsim;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Simulator;
using Appccelerate.EventBroker;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Aircrafts;

namespace Vatsim.Xpilot.Core
{
    public class InjectionModules : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserInterface>().ToFactory();
            Bind<ITabPages>().ToFactory();
            Bind<IEventBroker>().To(typeof(EventBroker)).InSingletonScope();
            Bind<IAppConfig>().To(typeof(AppConfig)).InSingletonScope();
            Bind<INetworkManager>().To(typeof(NetworkManager)).InSingletonScope();
            Bind<IAFVManaged>().To(typeof(AFVManaged)).InSingletonScope();
            Bind<ISoundManager>().To(typeof(SoundManager)).InSingletonScope();
            Bind<IXplaneAdapter>().To(typeof(XplaneAdapter)).InSingletonScope();
            Bind<IVersionCheck>().To(typeof(VersionCheck)).InSingletonScope();
            Bind<IUserAircraftManager>().To(typeof(UserAircraftManager)).InSingletonScope();
            Bind<IAircraftManager>().To(typeof(AircraftManager)).InSingletonScope();
            Bind<IControllerManager>().To(typeof(ControllerManager)).InSingletonScope();
            Bind<IControllerAtisManager>().To(typeof(ControllerAtisManager)).InSingletonScope();
        }
    }
}
