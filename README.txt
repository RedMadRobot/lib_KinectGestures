lib_KinectGestures — библиотека для распознавания жестов двух рук с помощью Microsoft Kinect

адрес библиотеки в GitHub — https://github.com/RedMadRobot/lib_KinectGestures

Для целей знакомства предусмотрен пример MobileGestures.sln

—————— Использование библиотеки ——————

1.	В References добавить
		KinectHandSwypes.dll
2.	Добавить директиву
		using KinectHandSwypes
3.	Объявить экземпляр класса HandSwypes:
		HandSwypes Hands = new HandSwypes();
4.	Подключить Kinect к проекту, как описано в Kinect SDK Documentation, настроиться на распознавание скелетов
5.	При получении сообщения SkeletonFrameReady от Kinect передать объект JointsCollection методу Iteration:
		Hands.Iteration(skeleton.Joints);
6.	Настроиться на сообщения от экземпляра класса HandSwypes и указать нужные действия в обработчиках

Метод класса ChangeMode позволяет выбрать режим распознавания жестов: SWYPE или CURSOR;
	в первом режиме класс отправляет сообщения о распознающихся жестах из категории вверх/вниз/вправо/влево/зум/нажатие для обеих рук
	во втором — сообщение с текущими координатами курсора MouseCoords

—————— Методы класса HandSwypes ——————

	void Iteration(JointsCollection Joints)
		— основной метод, с помощью которого передается информация о положении конечностей
	void ChangeMode(RecognitionMode InpMode)
	void ChangeMode(RecognitionMode InpMode, int ScreenWidth, int ScreenHeight)
		— методы для смены режима распознавания жестов
			InpMode = {SWYPE, CURSOR}
			ScreenWidth, ScreenHeight — принудительная установка разрешения экрана (в пикселях), к которому приводить движения курсора
										(по умолчанию берётся текущее разрешение основного монитора)

—————— Сообщения от HandSwypes ——————

	LSwypeRight	LSwypeRightComplete	RSwypeRight	RSwypeRightComplete			RPress
	LSwypeLeft	LSwypeLeftComplete	RSwypeLeft	RSwypeLeftComplete			LPress
	LSwypeUp	LSwypeUpComplete	RSwypeUp	RSwypeUpCompleteComplete	RPressComplete
	LSwypeDown	LSwypeDownComplete	RSwypeDown	RSwypeDownComplete			LPressComplete
	
	void LSwypeRight(object sender, ProgressEventArgs a)
		аргумент сообщений — прогресс текущего жеста (от 0 до 1)
	
	ZoomIn, ZoomOut	— аргументов не передают
	MouseCoords(x,y) — передает координаты курсора на экране
