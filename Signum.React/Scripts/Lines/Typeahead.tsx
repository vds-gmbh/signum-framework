﻿import * as React from 'react'



export interface TypeaheadProps {

    getItems: (query: string) => Promise<any[]>;
    getItemsTimeout?: number;
    minLength?: number;
    renderItem?: (item: any, query: string) => React.ReactNode;
    onSelect?: (item: any, e: React.SyntheticEvent) => string;
    scrollHeight?: number;
    autoSelect?: boolean;
    inputAttrs?: React.HTMLAttributes;
    menuAttrs?: React.HTMLAttributes;
    showHintOnFocus?: boolean;
    loadingMessage?: string;
    noResultsMessage?: string;
}

export interface TypeaheadState {
    shown?: boolean;
    items?: any[];
    query?: string;
    selectedIndex?: number;
}


export default class Typeahead extends React.Component<TypeaheadProps, TypeaheadState>
{
    constructor(props) {
        super(props);
        this.state = {
            shown: false,
            items: null,
            selectedIndex : null,
        };
    }

    static highlightedText = (val: string, query: string) => {

        var index = val.toLowerCase().indexOf(query.toLowerCase());
        if (index == -1)
            return val as React.ReactNode;

        return [
            val.substr(0, index),
            <strong key={0}>{val.substr(index, query.length) }</strong>,
            val.substr(index + query.length)
        ];
    }

    static defaultProps: TypeaheadProps = {
        getItems: null,
        getItemsTimeout: 300,
        minLength: 1,
        renderItem: Typeahead.highlightedText,
        onSelect: (elem, event) => elem,
        scrollHeight: 0,
        autoSelect: true,
        showHintOnFocus: false,
        loadingMessage: "Loading...",
        noResultsMessage: " - No results -"
    };
    

    handle: number;

    lookup() {
        if (!this.props.getItemsTimeout) {
            this.popupate();
        }
        else {
            if (this.handle)
                clearTimeout(this.handle);

            setTimeout(() => this.popupate(), this.props.getItemsTimeout);
        }
    }

    componentWillUnmount() {
        if (this.handle)
            clearTimeout(this.handle);
    }

    popupate() {

        if (this.input.value.length < this.props.minLength) {
            this.setState({ shown: false, items: null });
            return;
        }

        //this.setState({ shown: true, items: null });
               
        var query = this.input.value;
        this.props.getItems(query).then(items=> this.setState({
            items: items,
            shown: true,
            query: query,
            selectedIndex : 0,
        }));
    }

    select(e: React.SyntheticEvent) {
        this.input.value = this.props.onSelect(this.state.items[this.state.selectedIndex || 0], e);
        this.setState({ shown: false });
    }


    get input() {
        return this.refs["input"] as HTMLInputElement;
    }

    focused = false; 
    handleFocus = () => {
        if(!this.focused)
        {
            this.focused = true;
            if (this.props.minLength == 0 && !this.input.value || this.props.showHintOnFocus)
                this.lookup();
        }
    }

    handleBlur = () => {
        this.focused = false;

        if (!this.mouseover && this.state.shown)
            this.setState({ shown: false });
    }


    handleKeyDown = (e: React.KeyboardEvent) => {
        if (!this.state.shown) return;

        switch (e.keyCode) {
            case 9: // tab
            case 13: // enter
            case 27: // escape
                e.preventDefault();
                break;

            case 38: // up arrow
                e.preventDefault();
                var newIndex = ((this.state.selectedIndex || 0) - 1 + this.state.items.length) % this.state.items.length;
                this.setState({ selectedIndex: newIndex });
                break;

            case 40: // down arrow
                e.preventDefault();
                var newIndex = ((this.state.selectedIndex || 0) + 1) % this.state.items.length;
                this.setState({ selectedIndex: newIndex });
                break;
        }

        e.stopPropagation();
    }

    handleKeyUp = (e: React.KeyboardEvent) => {
        switch (e.keyCode)
        {
            case 40: // down arrow
            case 38: // up arrow
            case 16: // shift
            case 17: // ctrl
            case 18: // alt
                break;

            case 9: // tab
            case 13: // enter
                if (!this.state.shown) return;
                this.select(e);
                break;

            case 27: // escape
                if (!this.state.shown) return;
                this.setState({ shown: false });
                break;

            default:
                this.lookup();
        }
    }


    handleMenuClick = (e: React.MouseEvent) => {
        this.select(e);
        this.input.focus();
    }

    mouseover = true;
    handleElementMouseEnter = (event: React.MouseEvent) => {
        this.mouseover = true;
        this.setState({
            selectedIndex: parseInt((event.currentTarget as HTMLInputElement).getAttribute("data-index"))
        });
    }

    handleElementMouseLeave = (event: React.MouseEvent) => {
        this.mouseover = false;
        this.setState({ selectedIndex: null });
        if (!this.focused && this.state.shown)
            this.setState({ shown: false });
    }

    onMenuLoad = (ul: HTMLUListElement) => {

        var rec = this.input.getBoundingClientRect();
        if (ul) {
            ul.style.top = (rec.height) + "px";
            ul.style.left = "0px";
            ul.style.display = "block";
        }
    }

    render() {
        return <div>
            <input type="text" autoComplete="off" ref="input" {...this.props.inputAttrs}
                onFocus={this.handleFocus}
                onBlur={this.handleBlur}
                onKeyUp={this.handleKeyUp}
                onKeyDown={this.handleKeyDown}
                />
            <span>{/*placeholder for rouded borders*/}</span>
                {this.state.shown && <ul className="typeahead dropdown-menu" {...this.props.menuAttrs} ref={this.onMenuLoad}>
                    { /*!this.state.items ? <li className="loading"><a><small>{this.props.loadingMessage}</small></a></li> :*/
                    !this.state.items.length ? <li className="no-results"><a><small>{this.props.noResultsMessage}</small></a></li> :
                        this.state.items.map((item, i) => <li key={i} className={i == this.state.selectedIndex ? "active" : null} data-index={i}
                        onMouseEnter={this.handleElementMouseEnter}
                        onMouseLeave={this.handleElementMouseLeave}>
                    <a href="#" onClick={this.handleMenuClick}>{this.props.renderItem(item, this.state.query) }</a>
                        </li>) }
                    </ul>}
            </div>;
    }
}