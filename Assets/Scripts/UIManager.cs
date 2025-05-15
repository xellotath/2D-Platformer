using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
 public void PlayGame()
 {
  SceneManager.LoadScene(1);
 }

 public void Quit()
 {
  Application.Quit();
 }

 public void ClosePanel(GameObject panel)
 {
  panel.SetActive(false);
 }

 public void OpenPanel(GameObject panel)
 {
  panel.SetActive(true);
 }
 
}
