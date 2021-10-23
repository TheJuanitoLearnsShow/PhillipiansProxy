import io.jhdf.HdfFile
import org.jetbrains.kotlinx.dl.api.core.Sequential
import org.jetbrains.kotlinx.dl.api.core.loss.Losses
import org.jetbrains.kotlinx.dl.api.core.metric.Metrics
import org.jetbrains.kotlinx.dl.api.core.optimizer.Adam
import org.jetbrains.kotlinx.dl.dataset.image.ImageConverter
import java.io.File

fun main(args: Array<String>) {

    val labelsMap = mapOf(
        0 to "airplane",
        1 to "automobile",
        2 to "bird",
        3 to "cat"
    )

    val PATH_TO_IMAGE = "E:\\test\\s1.JPG"
    val imageArray = ImageConverter.toNormalizedFloatArray(File(PATH_TO_IMAGE))

    val PATH_TO_MODEL_JSON = "E:\\test\\mobilenet_v2_140_224\\nsfw_jpt_model.json"
    val modelConfig = File(PATH_TO_MODEL_JSON)
    val PATH_TO_WEIGHTS = "E:\\test\\mobilenet_v2_140_224\\nsfw_jpt_weights"
    val weights = File(PATH_TO_WEIGHTS)

    val model = Sequential.loadModelConfiguration(modelConfig, )

    model.use {
        it.compile(Adam(), Losses.SOFT_MAX_CROSS_ENTROPY_WITH_LOGITS, Metrics.ACCURACY)

        it.loadWeights(weights) //HdfFile(weights))

        val prediction = it.predict(imageArray)
        println("Predicted label is: $prediction. This corresponds to class ${labelsMap[prediction]}.")
    }


}