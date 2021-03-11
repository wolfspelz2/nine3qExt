﻿import { AbiItem } from 'web3-utils';

// tslint:disable: quotemark
export class ClaimsContract
{
    public static ABI = <Array<AbiItem>>[
        {
            "inputs": [
                {
                    "internalType": "address",
                    "name": "_proxyRegistryAddress",
                    "type": "address"
                }
            ],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "constructor"
        },
        {
            "anonymous": false,
            "inputs": [
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "approved",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "Approval",
            "type": "event"
        },
        {
            "anonymous": false,
            "inputs": [
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "operator",
                    "type": "address"
                },
                {
                    "indexed": false,
                    "internalType": "bool",
                    "name": "approved",
                    "type": "bool"
                }
            ],
            "name": "ApprovalForAll",
            "type": "event"
        },
        {
            "anonymous": false,
            "inputs": [
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "previousOwner",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "newOwner",
                    "type": "address"
                }
            ],
            "name": "OwnershipTransferred",
            "type": "event"
        },
        {
            "anonymous": false,
            "inputs": [
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "from",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "indexed": true,
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "Transfer",
            "type": "event"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "approve",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                }
            ],
            "name": "balanceOf",
            "outputs": [
                {
                    "internalType": "uint256",
                    "name": "",
                    "type": "uint256"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "baseURI",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "getApproved",
            "outputs": [
                {
                    "internalType": "address",
                    "name": "",
                    "type": "address"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "internalType": "address",
                    "name": "operator",
                    "type": "address"
                }
            ],
            "name": "isApprovedForAll",
            "outputs": [
                {
                    "internalType": "bool",
                    "name": "",
                    "type": "bool"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "isOwner",
            "outputs": [
                {
                    "internalType": "bool",
                    "name": "",
                    "type": "bool"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "_to",
                    "type": "address"
                }
            ],
            "name": "mintTo",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "name",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "owner",
            "outputs": [
                {
                    "internalType": "address",
                    "name": "",
                    "type": "address"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "ownerOf",
            "outputs": [
                {
                    "internalType": "address",
                    "name": "",
                    "type": "address"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [],
            "name": "renounceOwnership",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "from",
                    "type": "address"
                },
                {
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "safeTransferFrom",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "from",
                    "type": "address"
                },
                {
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                },
                {
                    "internalType": "bytes",
                    "name": "_data",
                    "type": "bytes"
                }
            ],
            "name": "safeTransferFrom",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "internalType": "bool",
                    "name": "approved",
                    "type": "bool"
                }
            ],
            "name": "setApprovalForAll",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "bytes4",
                    "name": "interfaceId",
                    "type": "bytes4"
                }
            ],
            "name": "supportsInterface",
            "outputs": [
                {
                    "internalType": "bool",
                    "name": "",
                    "type": "bool"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "symbol",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "uint256",
                    "name": "index",
                    "type": "uint256"
                }
            ],
            "name": "tokenByIndex",
            "outputs": [
                {
                    "internalType": "uint256",
                    "name": "",
                    "type": "uint256"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "owner",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "index",
                    "type": "uint256"
                }
            ],
            "name": "tokenOfOwnerByIndex",
            "outputs": [
                {
                    "internalType": "uint256",
                    "name": "",
                    "type": "uint256"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [
                {
                    "internalType": "uint256",
                    "name": "_tokenId",
                    "type": "uint256"
                }
            ],
            "name": "tokenURI",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "totalSupply",
            "outputs": [
                {
                    "internalType": "uint256",
                    "name": "",
                    "type": "uint256"
                }
            ],
            "payable": false,
            "stateMutability": "view",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "from",
                    "type": "address"
                },
                {
                    "internalType": "address",
                    "name": "to",
                    "type": "address"
                },
                {
                    "internalType": "uint256",
                    "name": "tokenId",
                    "type": "uint256"
                }
            ],
            "name": "transferFrom",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": false,
            "inputs": [
                {
                    "internalType": "address",
                    "name": "newOwner",
                    "type": "address"
                }
            ],
            "name": "transferOwnership",
            "outputs": [],
            "payable": false,
            "stateMutability": "nonpayable",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "baseTokenURI",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "pure",
            "type": "function"
        },
        {
            "constant": true,
            "inputs": [],
            "name": "contractURI",
            "outputs": [
                {
                    "internalType": "string",
                    "name": "",
                    "type": "string"
                }
            ],
            "payable": false,
            "stateMutability": "pure",
            "type": "function"
        }
    ];
    // public static ABI = <Array<AbiItem>>[
    //     {
    //         'inputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'constructor'
    //     },
    //     {
    //         'anonymous': false,
    //         'inputs': [
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_approved',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'Approval',
    //         'type': 'event'
    //     },
    //     {
    //         'anonymous': false,
    //         'inputs': [
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_operator',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': false,
    //                 'internalType': 'bool',
    //                 'name': '_approved',
    //                 'type': 'bool'
    //             }
    //         ],
    //         'name': 'ApprovalForAll',
    //         'type': 'event'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_approved',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'approve',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_to',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             },
    //             {
    //                 'internalType': 'string',
    //                 'name': '_data',
    //                 'type': 'string'
    //             }
    //         ],
    //         'name': 'mint',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'anonymous': false,
    //         'inputs': [
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': 'previousOwner',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': 'newOwner',
    //                 'type': 'address'
    //             }
    //         ],
    //         'name': 'OwnershipTransferred',
    //         'type': 'event'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_from',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'address',
    //                 'name': '_to',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'safeTransferFrom',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_from',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'address',
    //                 'name': '_to',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             },
    //             {
    //                 'internalType': 'bytes',
    //                 'name': '_data',
    //                 'type': 'bytes'
    //             }
    //         ],
    //         'name': 'safeTransferFrom',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_operator',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'bool',
    //                 'name': '_approved',
    //                 'type': 'bool'
    //             }
    //         ],
    //         'name': 'setApprovalForAll',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'anonymous': false,
    //         'inputs': [
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_from',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'address',
    //                 'name': '_to',
    //                 'type': 'address'
    //             },
    //             {
    //                 'indexed': true,
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'Transfer',
    //         'type': 'event'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_from',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'address',
    //                 'name': '_to',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'transferFrom',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_newOwner',
    //                 'type': 'address'
    //             }
    //         ],
    //         'name': 'transferOwnership',
    //         'outputs': [],
    //         'stateMutability': 'nonpayable',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             }
    //         ],
    //         'name': 'balanceOf',
    //         'outputs': [
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [],
    //         'name': 'CANNOT_TRANSFER_TO_ZERO_ADDRESS',
    //         'outputs': [
    //             {
    //                 'internalType': 'string',
    //                 'name': '',
    //                 'type': 'string'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'getApproved',
    //         'outputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '',
    //                 'type': 'address'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'getTokenData',
    //         'outputs': [
    //             {
    //                 'internalType': 'string',
    //                 'name': '',
    //                 'type': 'string'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'uint64',
    //                 'name': '_index',
    //                 'type': 'uint64'
    //             }
    //         ],
    //         'name': 'getTokenIdByOwnerAndIndex',
    //         'outputs': [
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             },
    //             {
    //                 'internalType': 'address',
    //                 'name': '_operator',
    //                 'type': 'address'
    //             }
    //         ],
    //         'name': 'isApprovedForAll',
    //         'outputs': [
    //             {
    //                 'internalType': 'bool',
    //                 'name': '',
    //                 'type': 'bool'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [],
    //         'name': 'name',
    //         'outputs': [
    //             {
    //                 'internalType': 'string',
    //                 'name': '_name',
    //                 'type': 'string'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [],
    //         'name': 'NOT_CURRENT_OWNER',
    //         'outputs': [
    //             {
    //                 'internalType': 'string',
    //                 'name': '',
    //                 'type': 'string'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [],
    //         'name': 'owner',
    //         'outputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '',
    //                 'type': 'address'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'uint256',
    //                 'name': '_tokenId',
    //                 'type': 'uint256'
    //             }
    //         ],
    //         'name': 'ownerOf',
    //         'outputs': [
    //             {
    //                 'internalType': 'address',
    //                 'name': '_owner',
    //                 'type': 'address'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [
    //             {
    //                 'internalType': 'bytes4',
    //                 'name': '_interfaceID',
    //                 'type': 'bytes4'
    //             }
    //         ],
    //         'name': 'supportsInterface',
    //         'outputs': [
    //             {
    //                 'internalType': 'bool',
    //                 'name': '',
    //                 'type': 'bool'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     },
    //     {
    //         'inputs': [],
    //         'name': 'symbol',
    //         'outputs': [
    //             {
    //                 'internalType': 'string',
    //                 'name': '_symbol',
    //                 'type': 'string'
    //             }
    //         ],
    //         'stateMutability': 'view',
    //         'type': 'function'
    //     }
    // ];
}
