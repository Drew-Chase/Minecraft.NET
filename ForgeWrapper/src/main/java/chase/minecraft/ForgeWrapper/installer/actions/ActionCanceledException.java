package chase.minecraft.ForgeWrapper.installer.actions;

public class ActionCanceledException extends Exception {
  ActionCanceledException(Exception parent) {
    super(parent);
  }
}
