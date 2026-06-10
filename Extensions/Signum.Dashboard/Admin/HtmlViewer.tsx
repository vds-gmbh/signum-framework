import * as React from 'react'
import { ReadonlyBinding } from "@framework/Lines";
import HtmlEditor from "../../Signum.HtmlEditor/HtmlEditor";

export function HtmlViewer(p: { text: string; htmlAttributes?: React.HTMLAttributes<HTMLDivElement> }): React.JSX.Element {

  var binding = new ReadonlyBinding(p.text, "");

  return (
    <div className="html-viewer" >
      <HtmlEditor readOnly
        binding={binding}
        htmlAttributes={p.htmlAttributes}
        small
      />
    </div>
  );
}
