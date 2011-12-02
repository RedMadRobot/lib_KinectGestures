lib_KinectGestures — библиотека для распознавания жестов двух рук с помощью Microsoft Kinect

адрес библиотеки в GitHub — https://github.com/RedMadRobot/lib_KinectGestures

Для целей знакомства предусмотрен пример MobileGestures.sln

————— I ——————
Если нужно завести новый проект на WPF, открываем пример MobileGestures.sln и
1.	чистим интерфейс (холст в MainWindow.xml)
2.	чистим обработчики событий (#region EventHandlers в файле MainWindow.xaml.cs)

————— II ——————
Если хотим начать пользоваться библиотекой в текущем проекте на .NET4 нужно:

1.	в References существующего проекта добавить
		Microsoft.Research.Kinect,
		Coding4Fun.Kinect.
2.	в файл с реализацией, в котором будем обрабатывать события с жестами, добавить в начало директивы
		using Microsoft.Research.Kinect.Nui;
		using Coding4Fun.Kinect.Wpf.Controls;
3.	добавить в проект файл HandSwypes.cs
4.	объявить экземпляр класса HandSwypes, он будет обрабатывать данные скелета с Kinect
		HandSwypes Hands = new HandSwypes();
5.	настроиться на получение сообщений о распознаваемых жестах из доступных:

		LSwypeRight			LSwypeRightComplete					ZoomIn				RSwypeRight			RSwypeRightComplete
		LSwypeLeft			LSwypeLeftComplete					ZoomOut				RSwypeLeft			RSwypeLeftComplete
		LSwypeUp			LSwypeUpCompleteComplete			MouseCoords			RSwypeUp			RSwypeUpCompleteComplete
		LSwypeDown			LSwypeDownComplete										RSwypeDown			RSwypeDownComplete
	
	например
		Hands.RSwypeRight += new ProgressEventHandler(_имя_обработчика_);
		
	сообщения передают прогресс текущего жеста (от 0.0 до 1.0), ZoomIn/ZoomOut не имеют аргументов, MouseCoords передает координаты курсора на экране (x,y)
	
6. 	настроиться на взаимодействие с Kinect:
		i.	описать экземпляр класса Runtime:
				Runtime nui;
		ii.	инициализировать Kinect перед работой:
				SetupKinect(int ElevateAngle);
		iii.настроить обработчик сообщения SkeletonFrameReady (см. пример),
		iv.	при закрытии программы вызвать
				nui.Uninitialize();