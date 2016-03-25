﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winterdom.Viasfora.Text {
  public class PresentationModeFontChanger {
    private VsfPackage package;
    private IVsfSettings settings;
    private FontCategory[] categories;
    private IVsFontAndColorStorage fontsAndColors;
    private bool enabled;

    public PresentationModeFontChanger(VsfPackage package) {
      this.package = package;
      this.enabled = false;
      this.settings = SettingsContext.GetSettings();
      this.categories = GetCategories();
      this.fontsAndColors = (IVsFontAndColorStorage)
        VsfPackage.GetGlobalService(typeof(IVsFontAndColorStorage));
    }

    public void TurnOn() {
      if ( !settings.PresentationModeIncludeEnvFonts )
        return;

      double zoomLevel = VsfPackage.GetPresentationModeZoomLevel();
      enabled = true;
      foreach ( var category in this.categories ) {
        TurnOnCategory(category, zoomLevel);
      }
    }
    public void TurnOff(bool notifyChanges = true) {
      if ( enabled ) {
        foreach ( var category in this.categories ) {
          TurnOffCategory(category, notifyChanges);
        }
      }
    }

    private void TurnOnCategory(FontCategory category, double zoomLevel) {
      Guid categoryId = category.Id;

      int hr = fontsAndColors.OpenCategory(
        ref categoryId,
        (uint)(__FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES | __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS)
        );

      if ( ErrorHandler.Succeeded(hr) ) {
        LOGFONTW[] logfont = new LOGFONTW[1];
        FontInfo[] fontInfo = new FontInfo[1];

        hr = fontsAndColors.GetFont(logfont, fontInfo);
        if ( ErrorHandler.Succeeded(hr) ) {
          category.FontInfo = fontInfo[0];
          double size = fontInfo[0].wPointSize;
          size = (size * zoomLevel) / 100;

          fontInfo[0].bFaceNameValid = 0;
          fontInfo[0].bCharSetValid = 0;
          fontInfo[0].bPointSizeValid = 1;
          fontInfo[0].wPointSize = Convert.ToUInt16(size);
          fontsAndColors.SetFont(fontInfo);
        }
        fontsAndColors.CloseCategory();
      }
    }

    private void TurnOffCategory(FontCategory category, bool notifyChanges) {
      Guid categoryId = category.Id;
      var flags = __FCSTORAGEFLAGS.FCSF_LOADDEFAULTS;
      if ( notifyChanges ) {
        flags |= __FCSTORAGEFLAGS.FCSF_PROPAGATECHANGES;
      }

      int hr = fontsAndColors.OpenCategory(ref categoryId, (uint)flags);

      if ( ErrorHandler.Succeeded(hr) ) {
        FontInfo[] fontInfo = new FontInfo[] {
          category.FontInfo
        };
        fontInfo[0].bFaceNameValid = 0;
        fontInfo[0].bCharSetValid = 0;
        fontInfo[0].bPointSizeValid = 1;
        fontsAndColors.SetFont(fontInfo);
        fontsAndColors.CloseCategory();
      }
    }

    private FontCategory[] GetCategories() {
      return new FontCategory[] {
        // Environment Font
        new FontCategory("1F987C00-E7C4-4869-8A17-23FD602268B0"),
        // Statement Completion
        new FontCategory("C1614BB1-734F-4A31-BD42-5AE6275E16D2"),
        // Editor Tooltip
        new FontCategory("A9A5637F-B2A8-422E-8FB5-DFB4625F0111"),
      };
    }


    class FontCategory {
      public Guid Id;
      public FontInfo FontInfo;

      public FontCategory(String categoryId) {
        this.Id = Guid.Parse(categoryId);
      }
    }
  }
}