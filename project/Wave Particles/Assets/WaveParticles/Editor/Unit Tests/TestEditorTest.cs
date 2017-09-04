using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class TestEditorTest
{

    /// <summary>
    /// This is the test to Test that tests are actually working.
    /// 
    /// </summary>
    [Test]
    public void EditorTest()
    {
        //Arrange
        var gameObject = new GameObject();

        //Act
        //Try to rename the GameObject
        var newGameObjectName = "My game object";
        gameObject.name = newGameObjectName;

        //Assert
        //The object has a new name
        Assert.AreEqual(newGameObjectName, gameObject.name);
    }

    /// <summary>
    /// Sanity test!!!
    /// </summary>
    [Test]
    public void EditorTest2()
    {
        Assert.IsTrue(true);
    }
}
