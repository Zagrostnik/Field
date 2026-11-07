# Field Mission — Unity 6 starter kit

Это стартовый набор скриптов и оригинальных процедурных WAV-заглушек для одиночной FPS-миссии.
Скопируйте папку `FieldMissionStarter` из архива целиком в папку `Assets` вашего Unity-проекта.

## Важно
- Скрипты рассчитаны на Unity 6 и старый Input Manager (`Input.GetAxis`, `Input.GetKey`). В Player Settings → Active Input Handling выберите **Both** или **Input Manager (Old)**.
- Для `MissionUI` нужен TextMeshPro (обычно уже установлен в Unity).
- WAV-файлы — процедурные заглушки для проверки подключения. Это не профессиональные полевые записи; позднее их можно заменить своими лицензированными звуками.
- Набор не является готовой сценой: объекты, ссылки, коллайдеры, UI и анимационные параметры нужно назначить в Inspector.

## Состав
- Core: GameManager (Singleton), GameEvents (Observer/event channel)
- Player: FirstPersonLook, PlayerMovement, PlayerHealth
- Weapons: WeaponController, IWeaponFireStrategy, HitscanFireStrategy (Strategy)
- Threats: ThreatBase, FPVDrone, DroneFactory (Factory), DroneSpawner, ArtilleryDirector
- Mission: MissionGoal, MissionProgress
- Pooling: SimpleObjectPool (Object Pool)
- UI: MenuController, MissionUI
- Interfaces: IDamageable, IThreat, IPoolable

## Быстрая сборка сцены Mission
1. Создайте `Player` (Capsule), добавьте CharacterController, PlayerMovement, FirstPersonLook, PlayerHealth.
2. Создайте дочерний `CameraPivot` на высоте примерно 1.65 м, внутрь добавьте Camera. Укажите CameraPivot в PlayerMovement и FirstPersonLook.
3. Создайте дочерний объект `Weapon` у камеры, поместите туда модель автомата. Добавьте WeaponController и назначьте Camera, WeaponVisual, Animator (если есть), AudioSource, клипы и muzzle flash.
4. Создайте пустой `GameSystems`, добавьте GameManager. В сцене должен быть один GameManager.
5. Создайте Drone prefab: модель дрона + Collider + Rigidbody (Is Kinematic) + FPVDrone. Создайте DroneFactory и DroneSpawner, задайте 2–4 точки появления вне поля зрения.
6. Создайте `ArtilleryDirector`, назначьте Player, PlayerMovement, AudioSources, свист и взрыв.
7. На конечной позиции создайте Trigger Collider и добавьте MissionGoal.
8. Добавьте MissionProgress, назначьте Player, StartPoint и GoalPoint.
9. Создайте UI Canvas: TMP_Text для патронов, Slider прогресса, две панели смерти/победы с кнопками Restart и BackToMenu. Добавьте MissionUI и свяжите ссылки.
10. В сцене MainMenu создайте Canvas с кнопкой Play → MenuController.Play и Quit → MenuController.Quit. В Build Profiles добавьте MainMenu и Mission, названия должны совпадать.
11. В сцене Mission установите GameManager initialState = Playing. В меню GameManager не нужен.

## Анимации автомата
В WeaponController используются необязательные Animator-параметры:
- Trigger `Fire`
- Trigger `Reload`
- Bool `Aiming`
Если имена параметров в вашем Animator Controller другие — измените их в коде или переименуйте параметры в Animator.
Для анимации перезарядки установите `reloadDuration` равным длине клипа.

## Настройки оружия
- Magazine Size = 30
- Reserve Ammo = 120
- RPM = 650
- Damage = 35
Для реалистичности отключите стрельбу через препятствия: настройте `hitMask`, исключив слой Player и слой оружия.

## Поведение артиллерии
Каждый обстрел выбирает случайную точку в пределах 50 м от текущего игрока. После свиста снаряд разрывается.
Если игрок не лёг, смертельный радиус по умолчанию 8 м; если лёг — 2.2 м. Эти игровые значения можно балансировать в Inspector.
ВНИМАНИЕ: текущая логика — игровая абстракция укрытия, а не физическая симуляция осколков.

## Что ещё нужно подключить/доработать
- Реальный рельеф и препятствия вашей карты, укрытия, старт/финиш.
- Назначить аудиоклипы и модели.
- Добавить анимации рук/оружия, прицеливание и UI-прицел.
- Проверить высоту дронов, точки спавна и интервалы атак.
- Для прогресса маршрут должен быть примерно прямым между StartPoint и GoalPoint. Для извилистого маршрута лучше использовать Waypoints.
