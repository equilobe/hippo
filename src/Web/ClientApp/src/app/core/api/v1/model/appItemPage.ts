/**
 * Hippo.Web
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: 1.0
 *
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */
import { AppItem } from './appItem';

export interface AppItemPage {
    items?: Array<AppItem> | null;
    totalItems?: number;
    pageIndex?: number | null;
    pageSize?: number | null;
    readonly isLastPage?: boolean | null;
}
