import * as React from 'react'
import * as Operations from '@framework/Operations'
import { useRootClose } from '@restart/ui'
import * as Finder from '@framework/Finder'
import { is, JavascriptMessage, toLite } from '@framework/Signum.Entities'
import { Toast, Button, ButtonGroup } from 'react-bootstrap'
import { DateTime } from 'luxon'
import { useAPI, useAPIWithReload, useForceUpdate, useUpdatedRef } from '@framework/Hooks';
import * as Navigator from '@framework/Navigator'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import * as WhatsNewClient from '../WhatsNewClient'
import "./WhatsNewDropdown.css"
import { Link } from 'react-router-dom';
import { classes, Dic } from '@framework/Globals'
import MessageModal from '@framework/Modals/MessageModal'
import { WhatsNewEntity, WhatsNewMessage, WhatsNewOperation, WhatsNewState } from '../Signum.Entities.WhatsNew'
import * as AppContext from "@framework/AppContext"
import { API, NumWhatsNews, WhatsNewFull, WhatsNewShort } from '../WhatsNewClient'
import { HtmlViewer } from '../Templates/WhatsNewHtmlEditor'
import { useHistory } from 'react-router-dom'
import { useEffect } from 'react' 

export default function WhatsNewDropdown(props: { keepRingingFor?: number }) {

  if (!Navigator.isViewable(WhatsNewEntity))
    return null;

  return <WhatsNewDropdownImp keepRingingFor={props.keepRingingFor ?? 10 * 1000} />;
}

function WhatsNewDropdownImp(props: { keepRingingFor: number }) {

  const forceUpdate = useForceUpdate();
  const [isOpen, setIsOpen] = React.useState<boolean>(false);

  const [showNews, setShowNews] = React.useState<number>(5);
  
  const isOpenRef = useUpdatedRef(isOpen);

  var [countResult, reloadCount] = useAPIWithReload<WhatsNewClient.NumWhatsNews>(() => WhatsNewClient.API.myNewsCount().then(res => {
    if (isOpenRef.current) {
      WhatsNewClient.API.myNews()
        .then(als => {
          setNews(als);
        });
    }

    return res;
  }), [], { avoidReset: true });

  const [whatsNew, setNews] = React.useState<WhatsNewShort[] | undefined>(undefined);

  function handleOnToggle() {

    if (!isOpen) {
      WhatsNewClient.API.myNews()
        .then(wn => setNews(wn));
    }

    setIsOpen(!isOpen);
  }

  function handleClickAll() {
    setIsOpen(false);
    AppContext.history.push("~/news/");
  }

  function handleOnCloseNews(toRemove: WhatsNewShort[]) {

    //Optimistic
    let wasClosed = false;
    if (whatsNew) {
      whatsNew.extract(a => toRemove.some(r => is(r.whatsNew, a.whatsNew)));
      if (whatsNew.length == 0) {
        setIsOpen(false);
        wasClosed = true;
      }
    }
    if (countResult)
      countResult.numWhatsNews -= 1;
    forceUpdate();

    API.setNewsLogRead(toRemove.map(r => r.whatsNew.id)).then(res => {
      if (!res) {
        MessageModal.showError(<div>The news couldn't be removed</div>);
      }
      // Pesimistic
      WhatsNewClient.API.myNews()
        .then(wn => {
          if (wasClosed && wn.length > 0)
            setIsOpen(true);

          setNews(wn);
        });

      reloadCount();
    }), [toRemove];
    }

  var newsGroups = whatsNew == null ? null : whatsNew.orderByDescending(w => w.creationDate);

  var divRef = React.useRef<HTMLDivElement>(null);

  useRootClose(divRef, () => setIsOpen(false), { disabled: !isOpen });

  return (
    <>
      <div className="nav-link sf-bell-container" onClick={handleOnToggle}>
        <FontAwesomeIcon icon="bullhorn" className={classes("sf-newspaper", isOpen && "open", countResult && countResult.numWhatsNews > 0 && "active")} />
        {countResult && countResult.numWhatsNews > 0 && <span className="badge btn-danger badge-pill sf-news-badge">{countResult.numWhatsNews}</span>}
      </div>
      {isOpen && <div className="sf-news-toasts" ref={divRef}>
        {newsGroups == null ? <Toast> <Toast.Body>{JavascriptMessage.loading.niceToString()}</Toast.Body></Toast> :

          <>
            {newsGroups.length == 0 && <Toast><Toast.Body>{WhatsNewMessage.YouDoNotHaveAnyUnreadNews.niceToString()}</Toast.Body></Toast>}

            {
              newsGroups.filter((gr, i) => i < showNews)
                .map(a => <WhatsNewToast whatsnew={a} key={a.whatsNew.id} onClose={handleOnCloseNews} refresh={reloadCount} setIsOpen={setIsOpen} />)
            }
            {
              newsGroups.length > showNews &&
              <Toast onClose={() => handleOnCloseNews(whatsNew!.map(a => a))}>
                <Toast.Header>
                    <small>{WhatsNewMessage.CloseAll.niceToString()}</small>
                </Toast.Header>
              </Toast>
            }
            <Toast>
              <Toast.Body style={{ textAlign: "center" }}>
                <a style={{ cursor: "pointer", color: "#114177" }}  onClick={() => handleClickAll()}>{WhatsNewMessage.AllMyNews.niceToString()}</a>
              </Toast.Body>
            </Toast>
          </>
        }
      </div>}
    </>
  );
}

export function WhatsNewToast(p: { whatsnew: WhatsNewShort, onClose: (e: WhatsNewShort[]) => void, refresh: () => void, className?: string; setIsOpen: (isOpen: boolean) => void })
{
  //ignoring open tags other than img
  function HTMLSubstring(text: string) {
    var substring = text.substring(0, 100);
    substring = substring.replace("<p>", "");
    substring = substring.replace("</p>", "");
    if (substring.contains("<img")) {
      var fullImageTag = substring.match(/(<img[^>] *)(\/>)/gmi);
      if (fullImageTag != undefined && fullImageTag.length >= 1) {
        return substring + "...";
      }
      else {
        return substring.substring(0, substring.indexOf("<img")) + "...";
      }
    }
    return substring + "...";
  }

  function handleClickPreviewPicture(e: React.MouseEvent) {
    e.preventDefault();
    p.setIsOpen(false);
    AppContext.history.push("~/newspage/" + p.whatsnew.whatsNew.id);
  }

  return (
    <Toast onClose={() => p.onClose([p.whatsnew])} className={p.className}>
      <Toast.Header>
        <strong className="me-auto">{p.whatsnew.title} {!Navigator.isReadOnly(WhatsNewEntity) && <small style={{ color: "#d50a30" }}>{(p.whatsnew.status == "Draft") ? p.whatsnew.status : undefined}</small>}</strong>
        <small>{DateTime.fromISO(p.whatsnew.creationDate!).toRelative()}</small>
      </Toast.Header>
      <Toast.Body style={{ whiteSpace: "pre-wrap" }}>
        <img onClick={handleClickPreviewPicture} src={AppContext.toAbsoluteUrl("~/api/whatsnew/previewPicture/" + p.whatsnew.whatsNew.id)} style={{ maxHeight: "30vh", cursor:"pointer", maxWidth: "10vw", margin: "0px 0px 0px 10px" }} />
        <HtmlViewer text={HTMLSubstring(p.whatsnew.description)} />
        <br />
        <Link onClick={handleClickPreviewPicture} to={"~/newspage/" + p.whatsnew.whatsNew.id}>{WhatsNewMessage.ReadFurther.niceToString()}</Link>
      </Toast.Body>
    </Toast>
  );
}

WhatsNewToast.icons = {} as { [alertTypeKey: string]: React.ReactNode };