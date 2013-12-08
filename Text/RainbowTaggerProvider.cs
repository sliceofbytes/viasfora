﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Winterdom.Viasfora.Tags;

namespace Winterdom.Viasfora.Text {

  [Export(typeof(IClassifierProvider))]
  [ContentType(CSharp.ContentType)]
  [ContentType(Cpp.ContentType)]
  [ContentType(JScript.ContentType)]
  [ContentType(JScript.ContentTypeVS2012)]
  [ContentType(VB.ContentType)]
  [ContentType(FSharp.ContentType)]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  [Name("Rainbow Classifier")]
  public class RainbowTaggerProvider : IClassifierProvider {
    [Import]
    internal IClassificationTypeRegistryService ClassificationRegistry = null;

    public IClassifier GetClassifier(ITextBuffer buffer) {
      return new RainbowTagger(buffer, ClassificationRegistry);
    }
  }
}
