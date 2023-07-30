package chase.minecraft.ForgeWrapper.installer.actions;

import java.io.File;
import java.util.function.Predicate;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.json.Artifact;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;

public class ExtractAction extends Action {
  public ExtractAction(InstallV1 profile, ProgressCallback monitor) {
    super(profile, monitor, true);
  }
  
  public boolean run(File target, Predicate<String> optionals, File Installer) {
    boolean result = true;
    String failed = "An error occurred extracting the files:";
    Artifact contained = this.profile.getPath();
    if (contained != null) {
      File file = new File(target, contained.getFilename());
      if (!DownloadUtils.extractFile(contained, file, null)) {
        result = false;
        failed = failed + "\n" + contained.getFilename();
      } 
    } 
    if (!result)
      error(failed); 
    return result;
  }
  
  public boolean isPathValid(File targetDir) {
    return (targetDir.exists() && targetDir.isDirectory());
  }
  
  public String getFileError(File targetDir) {
    return !targetDir.exists() ? "Target directory does not exist" : (!targetDir.isDirectory() ? "Target is not a directory" : "");
  }
  
  public String getSuccessMessage() {
    return "Extracted successfully";
  }
}
