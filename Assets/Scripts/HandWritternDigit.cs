using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using Unity.Sentis.Layers;
using System.Linq;

public class HandWritternDigit : MonoBehaviour
{
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private float[] results;
    [SerializeField] private FingerDrawing fingerDrawing;

    private Model runtimeModel;
    private IWorker worker;
    private TensorFloat inputTensor;

    public static Action<int> OnResult;


    private void Start()
    {
        /*string softmaxOutputName = "Softmax_Output";
        runtimeModel.AddLayer(new Softmax(softmaxOutputName, runtimeModel.outputs[0]));
        runtimeModel.outputs[0] = softmaxOutputName;*/

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
    }

    public void ExecuteModel(Texture2D inputTex)
    {
        inputTensor?.Dispose();
        inputTensor = TextureConverter.ToTensor(inputTex, 28, 28, 1);
        worker.Execute(inputTensor);

        TensorFloat outputTensor = worker.PeekOutput() as TensorFloat;
        outputTensor.MakeReadable();
        results = outputTensor.ToReadOnlyArray();

        OnResult.Invoke(results.ToList().IndexOf(Min(results)));

        outputTensor.Dispose();
        fingerDrawing.ClearTexture();
    }

    private void OnDisable()
    {
        inputTensor?.Dispose();
        worker.Dispose();
    }

    public float Min(float[] _results)
    {
        float min = _results[0];

        for (int i = 1; i < _results.Length; i++)
        {
            if (_results[i] > min)
            {
                min = _results[i];
            }
        }

        // Print the minimum value
        Debug.Log("The minimum value is: " + min);

        return min;
    }
}
