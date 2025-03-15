using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Pause : PanelHandler {

    public override void DoEnable(string targetStartButton)
    {
        base.DoEnable(targetStartButton);

		if (gameManager.Instance)
        {
			gameManager.Instance.setGameState(gameManager.enGameState.MENU);
        }
	}

	public override void DoClose()
	{
		Time.timeScale = 1f;	//Release our system from pause
		if (gameManager.Instance)
		{
			gameManager.Instance.setGameState(gameManager.enGameState.LEVELPLAYING);    //This won't totally be correct?
		}
		base.DoClose(); //if we don't put this here our menu will be dismissed before our code can execute (hypothetically)
	}
}
