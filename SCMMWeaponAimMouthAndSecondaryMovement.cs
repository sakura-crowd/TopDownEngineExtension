using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

/// <summary>
/// WeaponAim コンポーネントを操作し、マウスと右スティック両方から Aim を制御できるようにするコンポーネントです。
/// 使い方は、WeaponAim コンポーネントを持つゲームオブジェクトに本スクリプトを追加するだけです。
/// WeaponAim コンポーネントは TopDown Engine の Demos の武器のプレハブに付加されています。
/// 
/// マウス (AimControl.Mouth) または右スティック (AimControl.SecondaryMovement) の入力を Update イベント関数で監視し、
/// 入力を検知したら、同じゲームオブジェクトに付加された WeaponAim コンポーネントの AimControl を切り替えます。
/// その後は、WeaponAim コンポーネントに設定した Mouse, SecondaryMouse いずれかの入力をもとに Aim を更新してもらいます。
/// 
/// WeaponAim コンポーネントを操作するには、継承ではなく外部コンポーネントから操作します。 例) WeaponAutoAim2D, WeaponAuotAim3D
/// 参照 https://topdown-engine-docs.moremountains.com/weapons.html#auto-aim
/// 
/// Unity 2021.3.14f1、TopDown Engine 3.1.1 で動作を確認しました。
/// </summary>
public class SCMMWeaponAimMouthAndSecondaryMovement : MonoBehaviour
{

    /// <summary>
    /// 操作する WeaponAim コンポーネントです。同じゲームオブジェクト内にあることが前提です。
    /// WeaponAutoAim.cs
    /// </summary>
    protected WeaponAim _weaponAim;

    /// <summary>
    /// WeaponAim が紐づく Weapon コンポーネントです。
    /// TopDown Engine の武器は Weapon コンポーネント派生クラスをコンポーネントとして付加します。
    /// </summary>
    protected Weapon _weapon;

    /// <summary>
    /// 直前のマウスポインタの座標
    /// </summary>
    protected Vector2 _lastMousePosition;

    // Start is called before the first frame update
    /// <summary>
    /// 必要なコンポーネントを取得します。
    /// WeaponAutoAim も Start イベント関数でコンポーネントの取得を行っています。
    /// </summary>
    void Start()
    {
        // 操作対象の WeaponAim や、 InputManager と紐づいた Weapon を取得します。
        _weaponAim = this.gameObject.GetComponent<WeaponAim>();
        _weapon = this.gameObject.GetComponent<Weapon>();
        if (_weaponAim == null)
        {
            Debug.LogWarning(this.name + " : the SCMMWeaponAimMouthAndSecondaryMovement on this object requires that you add either a WeaponAim2D or WeaponAim3D component to your weapon.");
            return; // 同じゲームオブジェクトに必要なコンポーネント(WeaponAim 派生クラスのコンポーネント)が見つからなかったため正常に動作しません。
        }
    }

    // Update is called once per frame
    /// <summary>
    /// 毎回入力を確認します。
    /// </summary>
    void Update()
    {
        if (_weaponAim == null)
        {
            return; // 同じゲームオブジェクトに必要なコンポーネント(WeaponAim 派生クラスのコンポーネント)が見つからなかったため正常に動作しません。
        }

        SwitchAimControlMouseIfEntered();
        SwitchAimControlSecondaryMovementIfEntered();  // マウスと右スティックから同時に入力があれば後に呼び出される関数の処理が優先されます。

    }

    /// <summary>
    /// マウス(Mouth)の入力があった場合に WeaponAim.AimControl を Mouse に切り替えます。
    /// </summary>
    /// <returns></returns>
    void SwitchAimControlMouseIfEntered()
    {
        Vector2 mousePosition = InputManager.Instance.MousePosition;
        if (mousePosition != _lastMousePosition)    // マウスの座標が変わっていたら入力と判断します。
        {
            _lastMousePosition = mousePosition; // マウスの座標を更新します。

            _weaponAim.AimControl = WeaponAim.AimControls.Mouse;    // 直近の入力がマウスの場合は AimControl を Mouse に変え、WeaponAim の Update で Aim 位置を更新してもらう。

            return; // 入力を検知し、AimControl を切り替えました。
        }
        return;   // 入力を検知しませんでした。
    }

    /// <summary>
    /// ゲームパッド右スティック(SecondaryMovement)の入力があった場合に WeaponAim.AimControl を SecondaryMovement に切り替えます。
    /// </summary>
    /// <returns></returns>
    void SwitchAimControlSecondaryMovementIfEntered()
    {
        if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
        {
            return; // 呼び出しの前提条件が満たされていません。
        }

        // 武器の所持者に紐づく InputManager を取得。多人数プレイもあるため？
        InputManager inputManager = _weapon.Owner.LinkedInputManager;

        //　右スティックの有効な入力があれば、最後の有効な AIM 座標を置き換えます。
        // 参照 MoreMountains.TopDownEngine.InputManager の GetLastNonNullValues 関数
        Vector2 secondaryMovement = inputManager.SecondaryMovement;
        if (secondaryMovement.magnitude > inputManager.Threshold.x) // 右スティックの遊びの範囲を超えていれば入力と判断します。
        {
            _weaponAim.AimControl = WeaponAim.AimControls.SecondaryMovement;    // 直近の入力が右スティックの場合は AimControl を SecondaryMovement に変え、WeaponAim の Update で Aim 位置を更新してもらう。
            return; // 入力を検知し、AimControl を切り替えました。
        }

        return;   // 入力を検知しませんでした。
    }
}
