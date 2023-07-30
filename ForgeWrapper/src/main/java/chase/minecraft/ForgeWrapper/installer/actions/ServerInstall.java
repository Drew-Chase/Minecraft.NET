package chase.minecraft.ForgeWrapper.installer.actions;

import java.io.File;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.function.Predicate;
import chase.minecraft.ForgeWrapper.installer.DownloadUtils;
import chase.minecraft.ForgeWrapper.installer.SimpleInstaller;
import chase.minecraft.ForgeWrapper.installer.json.Artifact;
import chase.minecraft.ForgeWrapper.installer.json.InstallV1;
import chase.minecraft.ForgeWrapper.installer.json.Util;
import chase.minecraft.ForgeWrapper.installer.json.Version;

public class ServerInstall extends Action {
  private List<Artifact> grabbed = new ArrayList<>();
  
  public ServerInstall(InstallV1 profile, ProgressCallback monitor) {
    super(profile, monitor, false);
  }
  
  public boolean run(File target, Predicate<String> optionals, File installer) throws ActionCanceledException {
    if (target.exists() && !target.isDirectory()) {
      error("There is a file at this location, the server cannot be installed here!");
      return false;
    } 
    File librariesDir = new File(target, "libraries");
    if (!target.exists())
      target.mkdirs(); 
    librariesDir.mkdir();
    if (this.profile.getMirror() != null)
      this.monitor.stage(getSponsorMessage()); 
    checkCancel();
    Artifact contained = this.profile.getPath();
    if (contained != null) {
      this.monitor.stage("Extracting main jar:");
      if (!DownloadUtils.extractFile(contained, new File(target, contained.getFilename()), null)) {
        error("  Failed to extract main jar: " + contained.getFilename());
        return false;
      } 
      this.monitor.stage("  Extracted successfully");
    } 
    checkCancel();
    this.monitor.stage("Considering minecraft server jar");
    Map<String, String> tokens = new HashMap<>();
    tokens.put("ROOT", target.getAbsolutePath());
    tokens.put("MINECRAFT_VERSION", this.profile.getMinecraft());
    tokens.put("LIBRARY_DIR", librariesDir.getAbsolutePath());
    String path = Util.replaceTokens(tokens, this.profile.getServerJarPath());
    File serverTarget = new File(path);
    if (!serverTarget.exists()) {
      File parent = serverTarget.getParentFile();
      if (!parent.exists())
        parent.mkdirs(); 
      File versionJson = new File(target, this.profile.getMinecraft() + ".json");
      Version vanilla = Util.getVanillaVersion(this.profile.getMinecraft(), versionJson);
      if (vanilla == null) {
        error("Failed to download version manifest, can not find server jar URL.");
        return false;
      } 
      Version.Download server = vanilla.getDownload("server");
      if (server == null) {
        error("Failed to download minecraft server, info missing from manifest: " + versionJson);
        return false;
      } 
      versionJson.delete();
      if (!DownloadUtils.download(this.monitor, this.profile.getMirror(), server, serverTarget)) {
        serverTarget.delete();
        error("Downloading minecraft server failed, invalid checksum.\nTry again, or manually place server jar to skip download.");
        return false;
      } 
    } 
    checkCancel();
    List<File> libDirs = new ArrayList<>();
    File mcLibDir = new File(SimpleInstaller.getMCDir(), "libraries");
    if (mcLibDir.exists())
      libDirs.add(mcLibDir); 
    if (!downloadLibraries(librariesDir, optionals, libDirs))
      return false; 
    checkCancel();
    if (!this.processors.process(librariesDir, serverTarget, target, installer))
      return false; 
    return true;
  }
  
  public boolean isPathValid(File targetDir) {
    return (targetDir.exists() && targetDir.isDirectory() && (targetDir.list()).length == 0);
  }
  
  public String getFileError(File targetDir) {
    if (!targetDir.exists())
      return "The specified directory does not exist<br/>It will be created"; 
    if (!targetDir.isDirectory())
      return "The specified path needs to be a directory"; 
    return "There are already files at the target directory";
  }
  
  public String getSuccessMessage() {
    if (this.grabbed.size() > 0)
      return String.format("Successfully downloaded minecraft server, downloaded %d libraries and installed %s", new Object[] { Integer.valueOf(this.grabbed.size()), this.profile.getVersion() }); 
    return String.format("Successfully downloaded minecraft server and installed %s", new Object[] { this.profile.getVersion() });
  }
}
