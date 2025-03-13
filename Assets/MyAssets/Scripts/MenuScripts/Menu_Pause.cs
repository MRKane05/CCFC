using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Pause : PanelHandler {

    public override void DoEnable(string targetStartButton)
    {
        base.DoEnable(targetStartButton);

		Time.timeScale = 0.00001f;	 //So in theory we can handle all of our time control functionality on this menu
		if (gameManager.Instance)
        {
			gameManager.Instance.setGameState(gameManager.enGameState.MENU);
        }
	}

	public override void DoClose()
	{
		Time.timeScale = 1f;
		if (gameManager.Instance)
		{
			gameManager.Instance.setGameState(gameManager.enGameState.LEVELPLAYING);    //This won't totally be correct?
		}
		base.DoClose(); //if we don't put this here our menu will be dismissed before our code can execute (hypothetically)
	}
}
