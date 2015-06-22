using System;
using System.Collections.Generic;
using System.Linq;
using Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk.Model;



namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
	public sealed class MyStrategy : IStrategy
    {
        #region Static precumputed

	    private static bool _staticsInitialized = false;
	    private static Vector2D _fieldCenter;
        private static Vector2D _myGoalUp;
        private static Vector2D _myGoalDown;
        private static Vector2D _enemyGoalUp;
        private static Vector2D _enemyGoalDown;

        private static Rect2D _upperStrikeArea;
        private static Rect2D _lowerStrikeArea;

        private static Vector2D _defenderPosition;
        #endregion

        #region Privates
        //  автомат
        private readonly StackFsm _globalFsm;
        private bool _tickStart;

        //  состояние мира
        private World _world;
        private Game _game;
        private Move _move;
        private Hockeyist _self;

        //  частоиспользуемые данные о мире
        private Puck _puck;
        private Hockeyist _puckOwner;
        private Vector2D _enemyGoalkeeperPos;
        private Hockeyist _myGoalkeeper;
        private List<Hockeyist> _teammates;
        private List<Hockeyist> _enemies;
        private Vector2D _selfPosition;
        private Vector2D _selfSpeed;
        private Vector2D _selfLookDirection;
        #endregion

        #region State handlers

        #region idling
        //  отдых
        void IdleBehavior()
        {
            #region statecheck
            if (_tickStart)
            {
                if (_puckOwner == null)
                    StartPuckPersuing();
                else if (_puckOwner.IsTeammate)
                    StartAttacking();
                else
                    StartPuckStealing();
            }
            #endregion

            _tickStart = false;
            _globalFsm.Update();
        }
        #endregion

        #region attacking
        private StackFsm _attackingFsm;
        private Rect2D _currentStrikeArea;
        private Vector2D _strikeTarget;
        private int _swingingTicks;

        void StartAttacking()
        {
            if (_attackingFsm == null)
                _attackingFsm = new StackFsm();
            else
                _attackingFsm.ClearStack();

            if (_puckOwner.Id == _self.Id)  //  я с шайбой
            {
                _attackingFsm.SetCurrent(ApproachStrikePoint);
            }
            else   //  с шайбой партнер
            {
                _attackingFsm.SetCurrent(SupportAttacker);
            }

            _globalFsm.SetCurrent(AttackBehavior);
        }

        void AttackBehavior()
        {
            #region statecheck
            if (_tickStart)
            {
                if (_puckOwner == null)
                {
                    StartPuckPersuing();
                    _tickStart = false;
                    _globalFsm.Update();
                    return;
                }
                if (!_puckOwner.IsTeammate)
                {
                    StartPuckStealing();
                    _tickStart = false;
                    _globalFsm.Update();
                    return;
                }
            }

            #endregion

            ChooseAttackVector();
            _attackingFsm.Update();
        }

        //  выбор направления атаки
        void ChooseAttackVector()
        {
            double toUpperArea = Vector2D.Distance(_upperStrikeArea.Center, _selfPosition);
            double toLowerArea = Vector2D.Distance(_lowerStrikeArea.Center, _selfPosition);
            if (toLowerArea < toUpperArea)
            {
                _currentStrikeArea = _lowerStrikeArea;
                _strikeTarget = _enemyGoalUp;
            }
            else
            {
                _currentStrikeArea = _upperStrikeArea;
                _strikeTarget = _enemyGoalDown;
            }
        }

        //  движение до ворот
        void ApproachStrikePoint()
        {
            if (_currentStrikeArea.IsInside(_selfPosition))
            {
                _attackingFsm.SetCurrent(Aiming);
                _attackingFsm.Update();
                //return;
            }
            else
            {
                MoveTo(_currentStrikeArea.Center);
                //MoveToAvoiding(_currentStrikeArea.Center);
            }
        }

        //  прицеливание
        void Aiming()
        {
            double angle = _self.GetAngleTo(_strikeTarget.X, _strikeTarget.Y);

            if (Math.Abs(angle) < 0.01745329251994329576923690768489)
            {
                _attackingFsm.SetCurrent(SwingyStrike);
                _swingingTicks = 10;
                _attackingFsm.Update();
                return;
            }

            //_move.SpeedUp = -1.0;

            _move.Turn = angle;
        }

        //  удар с замахом
        void SwingyStrike()
        {
            if (_swingingTicks > 0 && _self.State != HockeyistState.Swinging)
                _move.Action = ActionType.Swing;
            else if (_self.SwingTicks >= _swingingTicks && _self.RemainingCooldownTicks <= 0)
            {
                _move.Action = ActionType.Strike;
                _globalFsm.SetCurrent(IdleBehavior);
            }
        }

        //  для хокеиста без шайбы
        void SupportAttacker()
        {
            _attackingFsm.SetCurrent(ApproachDefendPosition);
        }

        void ApproachDefendPosition()
        {
            if (_self.GetDistanceTo(_defenderPosition.X, _defenderPosition.Y) <= _self.Radius / 2)
                _attackingFsm.SetCurrent(guardGoal);
        }

        void guardGoal()
        {

        }
        #endregion  //  attacking

        #region puck stealing
        void StartPuckStealing()
        {
            _globalFsm.SetCurrent(StealPuckBehavior);
        }

        //  отжим шайбы у противника
        void StealPuckBehavior()
        {
            #region statecheck
            if (_tickStart)
            {
                if (_puckOwner == null)
                {
                    StartPuckPersuing();
                    _tickStart = false;
                    _globalFsm.Update();
                    return;
                }
                else if (_puckOwner.IsTeammate)
                {
                    StartAttacking();
                    _tickStart = false;
                    _globalFsm.Update();
                    return;
                }
            }
            #endregion

            var futurePuckPos = new Vector2D(_puck.X, _puck.Y) + new Vector2D(_puck.SpeedX, _puck.SpeedY) * 5;

            double angleToPuck = _self.GetAngleTo(futurePuckPos.X, futurePuckPos.Y);

            _move.Turn = angleToPuck;

            _move.SpeedUp = 1.0d;

            if (_self.GetDistanceTo(_puck) <= _game.StickLength && Math.Abs(_self.GetAngleTo(_puck)) <= _game.StickSector / 2)
                _move.Action = ActionType.TakePuck;
            else if (_self.GetDistanceTo(_puckOwner) <= _game.StickLength && Math.Abs(_self.GetAngleTo(_puckOwner)) <= _game.StickSector / 2)
                _move.Action = ActionType.Strike;
        }
        #endregion

        #region puck persuing
        void StartPuckPersuing()
        {
            _globalFsm.SetCurrent(PursuePuckBehavior);
        }

        //  забор безхозной шайбы
        void PursuePuckBehavior()
        {
            #region statecheck
            if (_tickStart)
            {
                if (_puckOwner != null)
                {
                    if (_puckOwner.IsTeammate)
                        StartAttacking();
                    else
                        StartPuckStealing();

                    _tickStart = false;
                    _globalFsm.Update();
                    return;
                }
            }
            #endregion

            Vector2D futurePuckPos = new Vector2D(_puck.X, _puck.Y) + new Vector2D(_puck.SpeedX, _puck.SpeedY) * 5;

            _move.Turn = _self.GetAngleTo(futurePuckPos.X, futurePuckPos.Y);

            _move.SpeedUp = 1.0d;

            if (_self.GetDistanceTo(_puck) <= _game.StickLength && Math.Abs(_self.GetAngleTo(_puck)) <= _game.StickSector / 2)
                _move.Action = ActionType.TakePuck;

            _tickStart = false;
        }
        #endregion
        
        #endregion

        //  вызывается системой каждый такт, для каждого хокеиста команды, кроме вратаря
        public void Move(Hockeyist self, World world, Game game, Move move)
		{
            UpdateData(self, world, game, move);

            if (!_staticsInitialized)
            {
                InitGameData(game, world);
                _staticsInitialized = true;
            }

            move.Action = ActionType.None;

            _tickStart = true;
            _globalFsm.Update();
		}

        #region initialization
        //  вызывается один раз при старте игры
        public MyStrategy()
        {
            _globalFsm = new StackFsm(IdleBehavior);
        }

        //  обновление динамичных данных о мире
        //  вызывается каждый такт
        void UpdateData(Hockeyist self, World world, Game game, Move move)
        {
            _self = self;
            _world = world;
            _game = game;
            _move = move;
            _puck = world.Puck;
            _puckOwner = GetHokeistById(_puck.OwnerHockeyistId);
            _teammates = new List<Hockeyist>(3);
            _enemies = new List<Hockeyist>(3);

            //  каждый такт - новые объекты хокеистов
            foreach (Hockeyist hok in world.Hockeyists)
            {
                if (hok.IsTeammate)
                {
                    if (hok.Type == HockeyistType.Goalie)
                        _myGoalkeeper = hok;
                    else
                        _teammates.Add(hok);
                }
                else
                {
                    if (hok.Type == HockeyistType.Goalie)
                        _enemyGoalkeeperPos = new Vector2D(hok.X, hok.Y);
                    else
                        _enemies.Add(hok);
                }
            }

            _selfPosition = new Vector2D(_self.X, _self.Y);
            _selfSpeed = new Vector2D(_self.SpeedX, _self.SpeedY);
            _selfLookDirection = new Vector2D(Math.Cos(-_self.Angle), Math.Sin(-_self.Angle));
        }

        //  инициализация статичных полей, один раз при старте игры
        void InitGameData(Game game, World world)
        {
            _myGoalUp.X = _myGoalDown.X = game.GoalNetWidth;
            _myGoalUp.Y = _enemyGoalUp.Y = game.GoalNetTop + 10;
            _myGoalDown.Y = _enemyGoalDown.Y = game.GoalNetTop + game.GoalNetHeight - 10;
            _enemyGoalUp.X = _enemyGoalDown.X = game.WorldWidth - game.GoalNetWidth;

            //  если свои ворота справа
            if (_myGoalkeeper.X > game.WorldWidth / 2)
            {
                Vector2D tmpUp = _myGoalUp;
                Vector2D tmpDown = _myGoalDown;

                _myGoalUp = _enemyGoalUp;
                _myGoalDown = _enemyGoalDown;
                _enemyGoalUp = tmpUp;
                _enemyGoalDown = tmpDown;

                _upperStrikeArea = new Rect2D(new Vector2D(275.0, 250.0), new Vector2D(375.0, 350.0));
                _lowerStrikeArea = new Rect2D(new Vector2D(275.0, 570.0), new Vector2D(375.0, 670.0));
            }
            else   // свои слева
            {
                _upperStrikeArea = new Rect2D(new Vector2D(675.0, 250.0), new Vector2D(925.0, 350.0));
                _lowerStrikeArea = new Rect2D(new Vector2D(675.0, 570.0), new Vector2D(925.0, 670.0));
            }

            _fieldCenter.X = game.WorldWidth / 2;
            _fieldCenter.Y = game.WorldHeight / 2;

        }
        #endregion  //  initialization

        #region Moving

        //  движение к точке
        public void MoveTo(Vector2D spot)
        {
            var selfSpeed = new Vector2D(_self.SpeedX, _self.SpeedY);
            Vector2D steer = Steerer.SeekForce(_selfPosition, selfSpeed, spot, 10.0);

            Vector2D op = _selfPosition + steer;
            double desiredAngle = _self.GetAngleTo(op.X, op.Y);

            _move.Turn = desiredAngle;
            _move.SpeedUp = 1.0d;
        }

        //  движение к точке с объезжанием врагов
        public void MoveToAvoiding(Vector2D spot)
        {
            Hockeyist closestEnemy = _enemies[0];
            double closestEnemyDist = _enemies[0].GetDistanceTo(_self);

            IEnumerable<Vector2D> threats = _enemies.Select(hok => new Vector2D(hok.X, hok.Y));
            IEnumerable<Vector2D>  threatSpeeds = _enemies.Select(hok => new Vector2D(hok.SpeedX, hok.SpeedY));

            //Vector2D seekForce = Steerer.SeekForce(_selfPosition, _selfSpeed, spot, 10);
            //Vector2d avoidanceForce = Steerer.AvoidForce(new Vector2d(puck.X, puck.Y), speed, threats, threatSpeeds, game.StickLength / 2);
            //Vector2D avoidanceForce = Steerer.AvoidForce(_selfPosition, _selfSpeed, threats, threatSpeeds, _game.StickLength / 2);
            Vector2D force = Steerer.SeekAvoidingForce(_selfPosition, _selfSpeed, spot, threats, threatSpeeds, _game.StickLength / 2);

            Vector2D or = new Vector2D(_self.X, _self.Y) + force;
            double desiredAngle = _self.GetAngleTo(or.X, or.Y);

            _move.Turn = desiredAngle;
            _move.SpeedUp = 1.0d;
        }
        #endregion

        #region Utility
        private Hockeyist GetHokeistById(long id)
        {
            if (id < 0)
                return null;

            return _world.Hockeyists.FirstOrDefault(hok => hok.Id == id);
        }

        private Player GetPlayerById(long id)
        {
            if (id < 0)
                return null;

            return _world.Players.FirstOrDefault(player => player.Id == id);
        }

        private double PuckSpeedAfterStrike(int swinging)
        {
            return _game.StruckPuckInitialSpeedFactor * (_game.StrikePowerBaseFactor + swinging * _game.StrikePowerGrowthFactor)
                + _selfSpeed.Magnitude * Math.Cos(_self.Angle - Vector2D.Angle(_selfSpeed, new Vector2D(1, 0)));
        }

        private Vector2D GetClosestGoalCorner()
        {
            double upperSpot = _self.GetDistanceTo(_enemyGoalUp.X, _enemyGoalUp.Y);
            double downSpot = _self.GetDistanceTo(_enemyGoalDown.X, _enemyGoalDown.Y);

            if (upperSpot < downSpot)
                return _enemyGoalUp;
            else
                return _enemyGoalDown;
        }

        private bool MayStrike(int swingingTicks, out Vector2D strikeTarget)
        {
            strikeTarget = new Vector2D(0, 0);

            double futurePuckSpeed = PuckSpeedAfterStrike(swingingTicks);

            double t = (_enemyGoalUp - new Vector2D(_puck.X, _puck.Y)).Magnitude / futurePuckSpeed;
            Vector2D golieFuture = _enemyGoalkeeperPos + (new Vector2D(0, -1) * (_game.GoalieMaxSpeed * t));

            if (!new Circle2D(golieFuture, 30).IsIntersects(new Circle2D(_enemyGoalUp, 20)))
            {
                strikeTarget = _enemyGoalUp;
                return true;
            }

            t = (_enemyGoalDown - new Vector2D(_puck.X, _puck.Y)).Magnitude / futurePuckSpeed;
            golieFuture = _enemyGoalkeeperPos + (new Vector2D(0, -1) * (_game.GoalieMaxSpeed * t));

            if (new Circle2D(golieFuture, 30).IsIntersects(new Circle2D(_enemyGoalDown, 20)))
                return false;
            else
            {
                strikeTarget = _enemyGoalDown;
                return true;
            }

            return false;
        }
        #endregion  //  Utility

    }
}   //  namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk

