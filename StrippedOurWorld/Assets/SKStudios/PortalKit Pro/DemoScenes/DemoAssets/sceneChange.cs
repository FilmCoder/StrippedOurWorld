using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SKStudios.Common.Demos {
    public class sceneChange : MonoBehaviour {
        void Start()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        int currentScene = 0;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene(++currentScene);
            if (Input.GetKeyDown(KeyCode.Backspace))
                SceneManager.LoadScene(--currentScene);
        }
    }

}
